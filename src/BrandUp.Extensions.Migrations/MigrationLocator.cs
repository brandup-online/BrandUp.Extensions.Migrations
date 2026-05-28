using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Default <see cref="IMigrationLocator"/> that discovers migrations by scanning assembly types for <see cref="MigrationAttribute"/>.
    /// </summary>
    public class MigrationLocator : IMigrationLocator
    {
        /// <inheritdoc />
        public IEnumerable<MigrationDefinition> FindMigrations(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var defs = new List<MigrationDefinition>();
            foreach (var type in GetLoadableTypes(assembly))
            {
                var migrationAttributes = type.GetCustomAttributes<MigrationAttribute>(false).ToArray();
                if (migrationAttributes.Length == 0)
                    continue;
                if (migrationAttributes.Length > 1)
                    throw new InvalidOperationException($"Type \"{type.FullName}\" declares multiple migration attributes; only one is allowed.");

                defs.Add(new MigrationDefinition(type, migrationAttributes[0]));
            }
            return defs;
        }

        static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                throw new InvalidOperationException($"Failed to load types from assembly \"{assembly.FullName}\" while scanning for migrations.", ex);
            }
        }
    }
}
