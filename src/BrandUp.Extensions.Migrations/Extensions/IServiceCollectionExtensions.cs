using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Registration helpers for the migration services.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="MigrationExecutor"/> and the default <see cref="IMigrationLocator"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="setup">Optional callback to configure <see cref="MigrationOptions"/>.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection AddMigrations(this IServiceCollection services, Action<MigrationOptions>? setup = null)
        {
            services.AddOptions<MigrationOptions>();

            if (setup != null)
                ConfigureMigrations(services, setup);

            services.TryAddTransient<MigrationExecutor>();
            services.TryAddTransient<IMigrationExecutor>(sp => sp.GetRequiredService<MigrationExecutor>());
            services.TryAddSingleton<IMigrationLocator, MigrationLocator>();

            return services;
        }

        /// <summary>
        /// Registers the migration services together with a scoped <typeparamref name="TState"/> as the <see cref="IMigrationState"/> store.
        /// </summary>
        /// <typeparam name="TState">The migration state store implementation.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="setup">Optional callback to configure <see cref="MigrationOptions"/>.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection AddMigrations<TState>(this IServiceCollection services, Action<MigrationOptions>? setup = null)
            where TState : class, IMigrationState
        {
            AddMigrations(services, setup);

            services.TryAddScoped<IMigrationState, TState>();

            return services;
        }

        /// <summary>
        /// Configures <see cref="MigrationOptions"/>. Can be called several times to add more assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Callback to configure <see cref="MigrationOptions"/>.</param>
        /// <returns>The same service collection for chaining.</returns>
        public static IServiceCollection ConfigureMigrations(this IServiceCollection services, Action<MigrationOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);

            services.Configure(configure);

            return services;
        }
    }
}
