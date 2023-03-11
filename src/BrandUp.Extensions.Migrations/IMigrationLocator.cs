using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationLocator
    {
        IEnumerable<MigrationDefinition> FindMigrations(Assembly assembly);
    }
}