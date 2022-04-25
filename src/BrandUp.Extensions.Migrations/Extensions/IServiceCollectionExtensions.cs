using BrandUp.Extensions.Migrations;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMigrations(this IServiceCollection services, Action<MigrationOptions> setup)
        {
            services.AddOptions<MigrationOptions>().Configure(setup);

            services.AddTransient<MigrationExecutor>();
            services.AddSingleton<IMigrationLocator, MigrationLocator>();

            return services;
        }
    }
}