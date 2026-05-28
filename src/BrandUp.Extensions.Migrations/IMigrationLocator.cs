using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Discovers migration definitions within an assembly.
    /// </summary>
    public interface IMigrationLocator
    {
        /// <summary>
        /// Finds all migration handlers declared in the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The migrations declared in the assembly.</returns>
        IEnumerable<MigrationDefinition> FindMigrations(Assembly assembly);
    }
}
