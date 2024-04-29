using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationLocator : IMigrationLocator
    {
        public IEnumerable<MigrationDefinition> FindMigrations(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            var defs = new List<MigrationDefinition>();
            foreach (var type in assembly.GetTypes())
            {
                var migrationAttribute = type.GetCustomAttribute<MigrationAttribute>();
                if (migrationAttribute == null)
                    continue;

                defs.Add(new MigrationDefinition(type, migrationAttribute));
            }
            return defs;
        }
    }
}