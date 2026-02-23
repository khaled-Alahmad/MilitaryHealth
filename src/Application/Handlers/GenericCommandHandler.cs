using MapsterMapper;
using MediatR;
using Application.Abstractions;
using Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;

public class GenericCommandHandler<TEntity, TDto> :
    IRequestHandler<CreateEntityCommand<TEntity, TDto>, TDto>,
    IRequestHandler<UpdateEntityCommand<TEntity, TDto>, TDto>,
    IRequestHandler<DeleteEntityCommand<TEntity>, bool>
    where TEntity : class
{
    private readonly IRepository<TEntity> _repo;
    private readonly IArchiveService _repoArch;

    private readonly IMapper _mapper;
    private readonly IFileNumberGenerator<TEntity>? _fileNumberGenerator;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public GenericCommandHandler(
        IRepository<TEntity> repo,
        IArchiveService repoArch,
        IMapper mapper,
        IFileNumberGenerator<TEntity>? fileNumberGenerator = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _repo = repo;
        _mapper = mapper;
        _repoArch = repoArch;
        _fileNumberGenerator = fileNumberGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TDto> Handle(CreateEntityCommand<TEntity, TDto> request, CancellationToken ct)
    {
        var entity = _mapper.Map<TEntity>(request.Dto)
            ?? throw new InvalidOperationException("Mapping produced null entity.");

        if (IsFinalDecisionEntity)
        {
            ApplyFinalDecisionCreateTimestamps(entity);
        }

        if (typeof(TEntity).Name.Contains("Applicant") && _fileNumberGenerator != null)
        {
            var fileProp = typeof(TEntity).GetProperty("FileNumber");
            if (fileProp != null && fileProp.CanWrite && fileProp.PropertyType == typeof(string))
            {
                var fileNumber = await _fileNumberGenerator.GenerateNextAsync(ct);
                fileProp.SetValue(entity, fileNumber);
            }

            var createdAtProp = typeof(TEntity).GetProperty("CreatedAt");
            if (createdAtProp != null && createdAtProp.CanWrite &&
                (createdAtProp.PropertyType == typeof(DateTime) || createdAtProp.PropertyType == typeof(DateTime?)))
            {
                var now = DateTime.UtcNow;
                createdAtProp.SetValue(entity, now);

                // set daily queue number starting from 1 each day
                var queueProp = typeof(TEntity).GetProperty("QueueNumber");
                if (queueProp != null && queueProp.CanWrite &&
                    (queueProp.PropertyType == typeof(int) || queueProp.PropertyType == typeof(int?)))
                {
                    try
                    {
                        var getNextQueueNumberMethod = _repo.GetType().GetMethod("GetNextQueueNumberAsync");
                        if (getNextQueueNumberMethod != null)
                        {
                            var task = (Task<int>)getNextQueueNumberMethod.Invoke(_repo, new object[] { now, ct });
                            var nextQueue = await task;
                            queueProp.SetValue(entity, nextQueue);
                        }
                    }
                    catch
                    {
                        // ignore any errors computing queue number and leave null
                    }
                }
            }
        }

        if (typeof(TEntity).Name.Contains("FinalDecision") 
            || typeof(TEntity).Name.Contains("SurgicalExam")
            || typeof(TEntity).Name.Contains("OrthopedicExam")
            || typeof(TEntity).Name.Contains("InternalExam")
            || typeof(TEntity).Name.Contains("EyeExam")
            )
        {
            var applicantFileProp = typeof(TEntity).GetProperty("ApplicantFileNumber");
            if (applicantFileProp != null && applicantFileProp.PropertyType == typeof(string))
            {
                var applicantFileNumber = applicantFileProp.GetValue(entity) as string;
                if (!string.IsNullOrWhiteSpace(applicantFileNumber))
                {
                    var existing = await _repo.GetByFileNumberAsync(applicantFileNumber, ct);
                    if (existing != null)
                    {
                        throw new InvalidOperationException("Applicant already registered before.");
                    }
                }
            }
        }


        await _repo.AddAsync(entity, ct);
        if (typeof(TEntity).Name.Contains("FinalDecision"))
        {
            var finalDecisionDto = _mapper.Map<FinalDecisionDto>(entity);
            await _repoArch.ArchiveFinalDecisionAsync(finalDecisionDto, ct);
        }

        return _mapper.Map<TDto>(entity);
    }

    public async Task<TDto> Handle(UpdateEntityCommand<TEntity, TDto> request, CancellationToken ct)
    {
        TEntity? entity;

        // Special handling for FinalDecision composite key using reflection (بدون مرجع مباشر للـ entity أو الـ DTO)
        if (typeof(TEntity).Name == "FinalDecision")
        {
            var dtoType = typeof(TDto);

            object GetKey(string propName)
            {
                var prop = dtoType.GetProperty(propName);
                if (prop == null)
                    throw new ArgumentException($"Property '{propName}' not found on DTO type '{dtoType.Name}'.");

                var value = prop.GetValue(request.Dto);
                if (value == null)
                    throw new ArgumentException($"Property '{propName}' on DTO '{dtoType.Name}' cannot be null when updating FinalDecision.");

                return value;
            }

            var keys = new object[]
            {
                request.Id,                  // DecisionID from route
                GetKey("OrthopedicExamID"),
                GetKey("SurgicalExamID"),
                GetKey("InternalExamID"),
                GetKey("EyeExamID"),
                GetKey("EarClinicID")
            };

            entity = await _repo.GetByIdAsync(keys, ct)
                     ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");
        }
        else
        {
            entity = await _repo.GetByIdAsync(request.Id, ct)
                     ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");
        }

        var dto = request.Dto;

        ApplyFinalDecisionUpdateTimestamps(entity, dto);

        // update only if value sent
        foreach (var prop in typeof(TDto).GetProperties())
        {
            var newValue = prop.GetValue(dto);
            if (newValue != null)
            {
                var entityProp = typeof(TEntity).GetProperty(prop.Name);
                if (entityProp != null)
                    entityProp.SetValue(entity, newValue);
            }
        }

        await _repo.UpdateAsync(entity, ct);
        return _mapper.Map<TDto>(entity);
    }


    public async Task<bool> Handle(DeleteEntityCommand<TEntity> request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(request.Id, ct);
        if (entity == null) return false;

        await _repo.DeleteAsync(entity, ct);
        return true;
    }

    private bool UserIsInRole(string role)
        => !string.IsNullOrWhiteSpace(role) && _httpContextAccessor?.HttpContext?.User?.IsInRole(role) == true;

    private bool IsFinalDecisionEntity
        => typeof(TEntity).Name.Contains("FinalDecision", StringComparison.OrdinalIgnoreCase);

    private bool CurrentUserIsReceptionist => UserIsInRole("Receptionist");
    private bool CurrentUserIsSupervisor => UserIsInRole("Supervisor");

    private void ApplyFinalDecisionCreateTimestamps(TEntity entity)
    {
        if (!IsFinalDecisionEntity)
            return;

        if (CurrentUserIsReceptionist)
        {
            SetDateTimeValue(entity, "ReceptionAddedAt", DateTime.UtcNow);
        }

        if (CurrentUserIsSupervisor)
        {
            var now = DateTime.UtcNow;
            SetDateTimeValue(entity, "SupervisorAddedAt", now);
            SetDateTimeValue(entity, "SupervisorLastModifiedAt", now, force: true);
        }
    }

    private void ApplyFinalDecisionUpdateTimestamps(TEntity entity, TDto dto)
    {
        if (!IsFinalDecisionEntity || dto == null)
            return;

        if (CurrentUserIsReceptionist &&
            !HasDateTimeValue(entity, "ReceptionAddedAt") &&
            !HasDateTimeValue(dto, "ReceptionAddedAt"))
        {
            SetDateTimeValue(dto, "ReceptionAddedAt", DateTime.UtcNow, force: true);
        }

        if (CurrentUserIsSupervisor)
        {
            var now = DateTime.UtcNow;
            if (!HasDateTimeValue(entity, "SupervisorAddedAt") &&
                !HasDateTimeValue(dto, "SupervisorAddedAt"))
            {
                SetDateTimeValue(dto, "SupervisorAddedAt", now, force: true);
            }

            SetDateTimeValue(dto, "SupervisorLastModifiedAt", now, force: true);
        }
    }

    private static bool HasDateTimeValue(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName);
        if (prop == null)
            return false;

        var value = prop.GetValue(target);
        if (value == null)
            return false;

        if (value is DateTime dt)
            return dt != default;

        if (value is DateTime?)
        {
            var ndt = (DateTime?)value;
            return ndt.HasValue && ndt.Value != default;
        }

        return false;
    }

    private static void SetDateTimeValue(object target, string propertyName, DateTime value, bool force = false)
    {
        var prop = target.GetType().GetProperty(propertyName);
        if (prop == null)
            return;

        var propType = prop.PropertyType;
        if (propType != typeof(DateTime) && propType != typeof(DateTime?))
            return;

        if (!force)
        {
            var current = prop.GetValue(target);
            if (current is DateTime dt && dt != default)
                return;
            if (current is DateTime?)
            {
                var ndt = (DateTime?)current;
                if (ndt.HasValue && ndt.Value != default)
                    return;
            }
        }

        if (propType == typeof(DateTime))
        {
            prop.SetValue(target, value);
        }
        else
        {
            prop.SetValue(target, (DateTime?)value);
        }
    }
}
