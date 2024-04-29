using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationOptions
    {
        readonly HashSet<Assembly> assemblies = new();

        public IEnumerable<Assembly> Assemblies => assemblies;

        public MigrationOptions AddAssembly(Assembly assembly)
        {
            if (!assemblies.Contains(assembly))
                assemblies.Add(assembly);

            return this;
        }
    }
}