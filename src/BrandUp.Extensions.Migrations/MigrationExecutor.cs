using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationExecutor
    {
        private readonly IMigrationLocator migrationLocator;
        private readonly IMigrationStore migrationStore;
        private readonly ILogger<MigrationExecutor> logger;
        private readonly IServiceProvider serviceProvider;

        public MigrationExecutor(IMigrationLocator migrationLocator, IMigrationStore migrationStore, ILogger<MigrationExecutor> logger, IServiceProvider serviceProvider)
        {
            this.migrationLocator = migrationLocator ?? throw new ArgumentNullException(nameof(migrationLocator));
            this.migrationStore = migrationStore ?? throw new ArgumentNullException(nameof(migrationStore));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<List<IMigrationVersion>> UpAsync(CancellationToken cancellationToken = default)
        {
            var currentVersion = await migrationStore.GetCurrentVersionAsync();
            if (currentVersion == null)
                currentVersion = new Version(0, 0, 0);

            var result = new List<IMigrationVersion>();
            var migrationDefinitions = migrationLocator.GetMigrations(currentVersion).ToList();
            if (migrationDefinitions.Count == 0)
            {
                logger.LogInformation($"Новых миграций не найдено. Текущая версия {currentVersion}.");
                return result;
            }

            var migrations = new SortedDictionary<MigrationDefinition, IMigration>();
            foreach (var migrationDefinition in migrationDefinitions)
            {
                var migration = CreateMigrationInstance(migrationDefinition);
                migrations.Add(migrationDefinition, migration);
            }

            foreach (var m in migrations)
            {
                await m.Value.UpAsync(cancellationToken);
                await migrationStore.ApplyMigration(m.Key);

                result.Add(m.Key);
            }

            return result;
        }

        public async Task<List<IMigrationVersion>> DownAsync(CancellationToken cancellationToken = default)
        {
            var appliedMigrations = await migrationStore.GetAppliedMigrationsAsync();

            var migrationDefinitions = new SortedDictionary<Version, MigrationDefinition>();
            foreach (var migrationDefinition in migrationLocator.GetMigrations(new Version(0, 0, 0)))
                migrationDefinitions.Add(migrationDefinition.Version, migrationDefinition);

            var migrations = new SortedDictionary<MigrationDefinition, IMigration>();
            foreach (var appliedMigration in appliedMigrations)
            {
                if (!migrationDefinitions.TryGetValue(appliedMigration.Version, out MigrationDefinition migrationDefinition))
                    throw new InvalidOperationException();

                var migration = CreateMigrationInstance(migrationDefinition);
                migrations.Add(migrationDefinition, migration);
            }

            var result = new List<IMigrationVersion>();
            foreach (var m in migrations)
            {
                await m.Value.DownAsync(cancellationToken);
                await migrationStore.CancelMigration(m.Key);

                result.Add(m.Key);
            }

            return result;
        }

        private IMigration CreateMigrationInstance(MigrationDefinition migrationDefinition)
        {
            var migrationType = migrationDefinition.MigrationType;
            var migrationConstructor = migrationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault();
            if (migrationConstructor == null)
                throw new InvalidOperationException();

            var constructorParamsInfo = migrationConstructor.GetParameters();
            var constratorParams = new object[constructorParamsInfo.Length];
            var i = 0;
            foreach (var p in constructorParamsInfo)
            {
                constratorParams[i] = serviceProvider.GetService(p.ParameterType);
                i++;
            }

            return (IMigration)migrationConstructor.Invoke(constratorParams);
        }
    }
}