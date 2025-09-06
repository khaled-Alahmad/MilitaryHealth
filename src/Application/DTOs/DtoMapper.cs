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
                    // إذا كان نوع الخاصية هو collection من نوع DTO
                    if (navValue is System.Collections.IEnumerable enumerable && dp.PropertyType.IsGenericType)
                    {
                        var dtoListType = dp.PropertyType.GetGenericArguments()[0];
                        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dtoListType));
                        foreach (var item in enumerable)
                        {
                            // استخدم Map بشكل recursive
                            var mappedItem = typeof(DtoMapper).GetMethod("Map")
                                .MakeGenericMethod(item.GetType(), dtoListType)
                                .Invoke(null, new object[] { item });
                            list.Add(mappedItem);
                        }
                        dp.SetValue(dto, list);
                    }
                    else
                    {
                        dp.SetValue(dto, navValue);
                    }
                }
            }
        }

        return dto;
    }
}
