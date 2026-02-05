using MapsterMapper;
using MediatR;
using Application.Abstractions;
using Application.DTOs;

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

    public GenericCommandHandler(
        IRepository<TEntity> repo,
        IArchiveService repoArch,
        IMapper mapper,
        IFileNumberGenerator<TEntity>? fileNumberGenerator = null)
    {
        _repo = repo;
        _mapper = mapper;
        _repoArch = repoArch;
        _fileNumberGenerator = fileNumberGenerator;
    }

    public async Task<TDto> Handle(CreateEntityCommand<TEntity, TDto> request, CancellationToken ct)
    {
        var entity = _mapper.Map<TEntity>(request.Dto)
            ?? throw new InvalidOperationException("Mapping produced null entity.");

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
                createdAtProp.SetValue(entity, DateTime.UtcNow);
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
}
