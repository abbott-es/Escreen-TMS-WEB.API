using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Resolver
{
    public static class EntityKeyResolver
    {
        public static string GetMappedKeyPropertyName<T>() where T : IEntity
        {
            var keyProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.EndsWith("ID") && p.PropertyType == typeof(Guid));

            return keyProperty?.Name
                ?? throw new InvalidOperationException($"No suitable key property found for {typeof(T).Name}");
        }
    }
}
