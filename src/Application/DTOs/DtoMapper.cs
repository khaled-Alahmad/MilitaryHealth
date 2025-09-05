namespace Application.DTOs;

public static class DtoMapper
{
    public static TDto Map<TEntity, TDto>(TEntity entity)
        where TEntity : class
        where TDto : class, new()
    {
        var dto = new TDto();

        var entityProps = typeof(TEntity).GetProperties();
        var dtoProps = typeof(TDto).GetProperties();

        foreach (var dp in dtoProps)
        {
            var ep = entityProps.FirstOrDefault(p => p.Name == dp.Name && dp.PropertyType.IsAssignableFrom(p.PropertyType));
            if (ep != null)
            {
                dp.SetValue(dto, ep.GetValue(entity));
            }
            else
            {
                // للـ navigation properties مثل List<RefractionDto>
                var nav = entityProps.FirstOrDefault(p => p.Name == dp.Name);
                if (nav != null)
                {
                    var navValue = nav.GetValue(entity);
                    dp.SetValue(dto, navValue);
                }
            }
        }

        return dto;
    }
}
