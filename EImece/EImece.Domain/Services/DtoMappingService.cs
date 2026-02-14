using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EImece.Domain.Services
{
    public static class DtoMappingService
    {
        public static TDestination MapTo<TDestination>(object source) where TDestination : class, new()
        {
            if (source == null)
            {
                return null;
            }

            var destination = new TDestination();
            var sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var sourceProperty in sourceProperties)
            {
                if (!destinationProperties.TryGetValue(sourceProperty.Name, out var destinationProperty))
                {
                    continue;
                }

                if (!destinationProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    continue;
                }

                var value = sourceProperty.GetValue(source, null);
                destinationProperty.SetValue(destination, value, null);
            }

            return destination;
        }

        public static List<TDestination> MapListTo<TDestination>(IEnumerable<object> source) where TDestination : class, new()
        {
            if (source == null)
            {
                return new List<TDestination>();
            }

            return source.Select(MapTo<TDestination>).Where(item => item != null).ToList();
        }

        public static List<TDestination> MapListTo<TSource, TDestination>(IEnumerable<TSource> source) where TDestination : class, new()
        {
            if (source == null)
            {
                return new List<TDestination>();
            }

            return source.Select(item => MapTo<TDestination>(item)).Where(item => item != null).ToList();
        }
    }
}
