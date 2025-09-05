using Application.Common.Extensions;
using Application.DTOs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

public class Repository<TEntity> : IPagedRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _db;
    //_mapper
    protected readonly DbSet<TEntity> _set;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Repository(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _set = _db.Set<TEntity>();
        _httpContextAccessor = httpContextAccessor;
    }

    private int? CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var claim = user.FindFirst("sub")
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("id");

            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    private string GetEntityKey(TEntity entity)
    {
        var keyProperty = _db.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.FirstOrDefault();
        return keyProperty != null ? _db.Entry(entity).Property(keyProperty.Name)?.CurrentValue?.ToString() ?? "" : "";
    }

    private string[]? GetChangedColumns(TEntity oldEntity, TEntity newEntity)
    {
        var changes = typeof(TEntity)
            .GetProperties()
            .Where(p => !Equals(p.GetValue(oldEntity), p.GetValue(newEntity)))
            .Select(p => p.Name)
            .ToArray();

        return changes.Length > 0 ? changes : null;
    }

    private string? GetOldValues(TEntity oldEntity, string[]? changedColumns)
    {
        if (changedColumns == null) return null;

        var dict = changedColumns.ToDictionary(
            col => col,
            col => oldEntity.GetType().GetProperty(col)?.GetValue(oldEntity)
        );

        return JsonSerializer.Serialize(dict);
    }

    private async Task LogAuditAsync(TEntity entity, string action, TEntity? oldEntity = null)
    {
        var changedColumns = oldEntity != null ? GetChangedColumns(oldEntity, entity) : null;

        var audit = new AuditLog
        {
            UserId = CurrentUserId,
            EntityName = typeof(TEntity).Name,
            EntityKey = GetEntityKey(entity),
            Action = action,
            ChangedColumns = changedColumns != null ? string.Join(",", changedColumns) : null,
            OldValues = oldEntity != null ? GetOldValues(oldEntity, changedColumns) : null,
            NewValues = JsonSerializer.Serialize(entity),
            Timestamp = DateTime.UtcNow
        };

        await _db.Set<AuditLog>().AddAsync(audit);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        
        await _set.AddAsync(entity, ct);
        await LogAuditAsync(entity, "Create");
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        var entityType = _db.Model.FindEntityType(typeof(TEntity));
        if (entityType == null)
            throw new InvalidOperationException($"EF model does not contain entity {typeof(TEntity).Name}.");

        var keyProps = entityType.FindPrimaryKey()?.Properties;
        if (keyProps == null || keyProps.Count == 0)
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} has no primary key defined.");

        var keyValues = new object[keyProps.Count];
        for (int i = 0; i < keyProps.Count; i++)
        {
            var propName = keyProps[i].Name;

            object? val = null;
            try
            {
                val = _db.Entry(entity).Property(propName).CurrentValue;
            }
            catch
            {
            }

            if (val == null)
            {
                var clrProp = typeof(TEntity).GetProperty(propName);
                if (clrProp != null)
                    val = clrProp.GetValue(entity);
            }

            if (val == null)
                throw new InvalidOperationException($"Key property '{propName}' value is null for entity {typeof(TEntity).Name}.");

            keyValues[i] = val;
        }

        var oldEntity = await _set.FindAsync(keyValues, ct);
        if (oldEntity == null)
            throw new InvalidOperationException("Entity not found in database.");

        var oldSnapshot = _db.Entry(oldEntity).CurrentValues.ToObject() as TEntity
                          ?? throw new InvalidOperationException("Failed to capture old entity values.");

        _db.Entry(oldEntity).CurrentValues.SetValues(entity);

        await LogAuditAsync(oldEntity, "Update", oldSnapshot);

        await _db.SaveChangesAsync(ct);
    }


    public async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        await LogAuditAsync(entity, "Delete", entity);
        _set.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public IQueryable<TEntity> Query() => _set.AsNoTracking();


    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        var entityType = _db.Model.FindEntityType(typeof(TEntity));
        if (entityType == null)
            throw new InvalidOperationException($"EF model does not contain entity {typeof(TEntity).Name}.");

        var keyProperties = entityType.FindPrimaryKey()?.Properties;
        if (keyProperties == null || keyProperties.Count == 0)
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} has no primary key defined.");

        if (keyProperties.Count == 1)
        {
            return await _set.FindAsync(new object[] { id }, ct);
        }

        if (id is object[] keys && keys.Length == keyProperties.Count)
        {
            return await _set.FindAsync(keys, ct);
        }

        var candidates = new[]
        {
        $"{typeof(TEntity).Name}ID",
        $"{typeof(TEntity).Name}Id",
        "Id",
        "ID"
    };

        var candidateKey = keyProperties
            .FirstOrDefault(k => candidates.Any(c => string.Equals(k.Name, c, StringComparison.OrdinalIgnoreCase)))
            ?? keyProperties.FirstOrDefault(k => string.Equals(k.Name, $"{typeof(TEntity).Name}ID", StringComparison.OrdinalIgnoreCase));

        if (candidateKey != null)
        {
            var targetType = candidateKey.ClrType;
            object? converted;
            try
            {
                if (targetType == typeof(Guid))
                {
                    converted = Guid.TryParse(id?.ToString(), out var g) ? g : throw new ArgumentException("Invalid Guid value.");
                }
                else if (targetType.IsEnum)
                {
                    converted = Enum.Parse(targetType, id!.ToString()!, ignoreCase: true);
                }
                else
                {
                    converted = Convert.ChangeType(id, targetType);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Unable to convert id to key type {targetType.Name}: {ex.Message}", ex);
            }

            var param = Expression.Parameter(typeof(TEntity), "e");
            var efPropertyMethod = typeof(EF).GetMethod("Property", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(targetType);
            var propertyAccess = Expression.Call(efPropertyMethod, param, Expression.Constant(candidateKey.Name));
            var constant = Expression.Constant(converted, targetType);
            var body = Expression.Equal(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);

            return await _set.AsNoTracking().FirstOrDefaultAsync(lambda, ct);
        }

        throw new ArgumentException($"Entity {typeof(TEntity).Name} has a composite key of {keyProperties.Count} values. Pass an object[] with the correct number of key values or provide a matching single key property (e.g. {typeof(TEntity).Name}ID).");
    }

    private static Expression<Func< TEntity, bool>>? BuildFilter(Dictionary<string, string>? filters)
    {
        if (filters == null || !filters.Any())
            return null;

        ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
        Expression? body = null;

        foreach (var kvp in filters)
        {
            string key = kvp.Key;
            string value = kvp.Value;

            // تمييز بين Min[Field] / Max[Field] / From[Field] / To[Field]
            string fieldName = key;
            string mode = "eq";

            if (key.StartsWith("Min["))
            {
                fieldName = key[4..^1];
                mode = "min";
            }
            else if (key.StartsWith("Max["))
            {
                fieldName = key[4..^1];
                mode = "max";
            }
            else if (key.StartsWith("From["))
            {
                fieldName = key[5..^1];
                mode = "from";
            }
            else if (key.StartsWith("To["))
            {
                fieldName = key[3..^1];
                mode = "to";
            }

            var prop = typeof(TEntity).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) continue;

            var left = Expression.Property(param, prop);
            Expression? condition = null;

            // محاولة تحويل value إلى نوع الحقل
            object? typedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            var right = Expression.Constant(typedValue);

            switch (mode)
            {
                case "eq":
                    if (prop.PropertyType == typeof(string))
                    {
                        // e.FullName.Contains(value)
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        condition = Expression.Call(left, containsMethod!, right);
                    }
                    else
                    {
                        condition = Expression.Equal(left, right);
                    }
                    break;

                case "min":
                case "from":
                    condition = Expression.GreaterThanOrEqual(left, right);
                    break;

                case "max":
                case "to":
                    condition = Expression.LessThanOrEqual(left, right);
                    break;
            }

            body = body == null ? condition : Expression.AndAlso(body, condition);
        }

        if (body == null) return null;

        return Expression.Lambda<Func<TEntity, bool>>(body, param);
    }

    public async Task<PagedResult<TEntity>> GetPagedAsync(
      Expression<Func<TEntity, bool>>? filter = null,
      string? sortBy = null,
      bool sortDesc = false,
      int page = 1,
      int pageSize = 20,
      CancellationToken ct = default,
      params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _set.AsNoTracking();

        if (filter != null)
            query = query.Where(filter);

        if (includes != null && includes.Any())
            query = includes.Aggregate(query, (current, include) => current.Include(include));

        if (!string.IsNullOrEmpty(sortBy))
            query = query.OrderByProperty(sortBy, sortDesc);

        var totalCount = await query.CountAsync(ct);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<TEntity>
        {
            Items = entities,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }



    public async Task<TEntity?> GetByFileNumberAsync(object id, CancellationToken ct = default)
    {
        if (id == null) return default;

        var fileNumber = id.ToString();
        if (string.IsNullOrWhiteSpace(fileNumber)) return default;

        var prop = typeof(TEntity).GetProperty("ApplicantFileNumber")
                ?? typeof(TEntity).GetProperty("FileNumber");

        if (prop == null)
        {
            return default;
        }


        return await _set.AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, prop.Name) == fileNumber, ct);
    }
    public async Task<TEntity?> GetByFileNumberAsync(string fileNumber, CancellationToken ct = default)
    {
       
        return await _set
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "ApplicantFileNumber") == fileNumber, ct);
    }
}
