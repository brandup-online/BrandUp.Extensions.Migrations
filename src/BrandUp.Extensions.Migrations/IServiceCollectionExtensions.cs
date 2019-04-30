using BrandUp.Extensions.Migrations;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMigrations(this IServiceCollection services, Assembly assembly)
        {
            services.AddTransient<MigrationExecutor>();
            services.AddSingleton<IMigrationLocator>(new MigrationLocator(assembly));

            return services;
        }
    }
}