using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Configures which assemblies are scanned for migration handlers.
    /// </summary>
    public class MigrationOptions
    {
        readonly HashSet<Assembly> assemblies = [];

        /// <summary>
        /// Assemblies registered for migration discovery.
        /// </summary>
        public IEnumerable<Assembly> Assemblies => assemblies;

        /// <summary>
        /// Registers an assembly to scan for migration handlers. Duplicate registrations are ignored.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The same <see cref="MigrationOptions"/> instance for chaining.</returns>
        public MigrationOptions AddAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);

            assemblies.Add(assembly);

            return this;
        }
    }
}
