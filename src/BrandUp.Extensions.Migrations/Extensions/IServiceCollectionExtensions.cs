using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Extensions.Migrations
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMigrations(this IServiceCollection services, Action<MigrationOptions> setup = null)
        {
            services.AddOptions<MigrationOptions>();

            if (setup != null)
                ConfigureMigrations(services, setup);

            services.AddTransient<MigrationExecutor>();
            services.AddSingleton<IMigrationLocator, MigrationLocator>();

            return services;
        }

        public static IServiceCollection AddMigrations<TState>(this IServiceCollection services, Action<MigrationOptions> setup = null)
            where TState : class, IMigrationState
        {
            AddMigrations(services, setup);

            services.AddScoped<IMigrationState, TState>();

            return services;
        }

        public static IServiceCollection ConfigureMigrations(this IServiceCollection services, Action<MigrationOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            services.Configure(configure);

            return services;
        }
    }
}