using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EImece.Domain.Models.DtoModels
{
    public static class EntityDtoMapper
    {
        public static TDto ToDto<TDto>(this object source) where TDto : class, new()
        {
            if (source == null)
            {
                return null;
            }

            var dto = new TDto();
            var sourceType = source.GetType();
            var targetType = typeof(TDto);
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.CanRead)
                                             .ToDictionary(p => p.Name, StringComparer.Ordinal);

            foreach (var targetProperty in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
            {
                if (sourceProperties.TryGetValue(targetProperty.Name, out var sourceProperty))
                {
                    if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                    {
                        var value = sourceProperty.GetValue(source);
                        targetProperty.SetValue(dto, value);
                    }
                }
            }

            return dto;
        }

        public static List<TDto> ToDtoList<TDto>(this IEnumerable<object> source) where TDto : class, new()
        {
            return source?.Select(ToDto<TDto>).ToList() ?? new List<TDto>();
        }
    }
}
