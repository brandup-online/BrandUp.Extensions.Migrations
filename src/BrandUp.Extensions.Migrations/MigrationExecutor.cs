using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        readonly MigrationOptions options;
        readonly IMigrationLocator migrationLocator;
        readonly IMigrationState migrationState;
        readonly ILogger<MigrationExecutor> logger;
        readonly IServiceProvider serviceProvider;

        public MigrationExecutor(IOptions<MigrationOptions> options, IMigrationLocator migrationLocator, IMigrationState migrationState, ILogger<MigrationExecutor> logger, IServiceProvider serviceProvider)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.migrationLocator = migrationLocator ?? throw new ArgumentNullException(nameof(migrationLocator));
            this.migrationState = migrationState ?? throw new ArgumentNullException(nameof(migrationState));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<List<IMigrationDefinition>> UpAsync(CancellationToken cancellationToken = default)
        {
            var structure = BuildStructure();
            if (structure == null)
            {
                logger.LogInformation($"Not found new migrations.");
                return new List<IMigrationDefinition>();
            }

            using var scope = serviceProvider.CreateScope();

            structure.Build(scope.ServiceProvider);

            return await structure.UpAsync(migrationState, cancellationToken);
        }
        public async Task<List<IMigrationDefinition>> DownAsync(CancellationToken cancellationToken = default)
        {
            var structure = BuildStructure();
            if (structure == null)
            {
                logger.LogInformation($"Not found new migrations.");
                return new List<IMigrationDefinition>();
            }

            using var scope = serviceProvider.CreateScope();

            structure.Build(scope.ServiceProvider);

            return await structure.DownAsync(migrationState, cancellationToken);
        }

        private MigrationStructure BuildStructure()
        {
            var migrationDefinitions = new HashSet<MigrationDefinition>();
            var assemblies = new List<Assembly>();
            foreach (var assembly in options.Assemblies)
                FindMigrations(migrationDefinitions, assembly, assemblies);

            if (migrationDefinitions.Count == 0)
                return null;

            return new MigrationStructure(migrationDefinitions);
        }
        private void FindMigrations(HashSet<MigrationDefinition> migrationDefinitions, Assembly assembly, List<Assembly> assemblies)
        {
            if (assemblies.Contains(assembly))
                return;
            assemblies.Add(assembly);

            foreach (var m in migrationLocator.FindMigrations(assembly))
            {
                if (migrationDefinitions.Contains(m))
                    continue;

                migrationDefinitions.Add(m);

                if (!m.IsRoot && !assemblies.Contains(m.ParentHandlerType.Assembly))
                    FindMigrations(migrationDefinitions, m.ParentHandlerType.Assembly, assemblies);
            }
        }
    }

    class MigrationStructure
    {
        readonly List<MigrationDefinition> migrations = new List<MigrationDefinition>();
        readonly Dictionary<Type, int> migrationTypes = new Dictionary<Type, int>();
        readonly Dictionary<string, int> migrationNames = new Dictionary<string, int>();
        readonly List<MigrationDefinition> roots = new List<MigrationDefinition>();
        readonly Dictionary<MigrationDefinition, MigrationDefinition> parents = new Dictionary<MigrationDefinition, MigrationDefinition>();
        readonly Dictionary<MigrationDefinition, List<MigrationDefinition>> childs = new Dictionary<MigrationDefinition, List<MigrationDefinition>>();
        readonly Dictionary<MigrationDefinition, IMigrationHandler> handlers = new Dictionary<MigrationDefinition, IMigrationHandler>();

        public MigrationStructure(IEnumerable<MigrationDefinition> migrationDefinitions)
        {
            foreach (var m in migrationDefinitions)
            {
                var index = migrations.Count;
                migrations.Add(m);
                migrationTypes.Add(m.HandlerType, index);
                migrationNames.Add(m.Name.ToUpper(), index);

                if (m.IsRoot)
                    roots.Add(m);
            }

            foreach (var migration in migrations)
            {
                if (migration.IsRoot)
                    continue;

                if (!TryGetByHandlerType(migration.ParentHandlerType, out MigrationDefinition parentMigration))
                    throw new InvalidOperationException();

                parents.Add(migration, parentMigration);

                if (!childs.TryGetValue(parentMigration, out List<MigrationDefinition> childMigrations))
                    childs.Add(parentMigration, childMigrations = new List<MigrationDefinition>());

                childMigrations.Add(migration);
            }
        }

        public void Build(IServiceProvider serviceProvider)
        {
            foreach (var m in migrations)
            {
                var handler = CreateMigrationHandler(m, serviceProvider);
                handlers.Add(m, handler);
            }
        }

        public async Task<List<IMigrationDefinition>> UpAsync(IMigrationState migrationState, CancellationToken cancellationToken)
        {
            var result = new List<IMigrationDefinition>();

            foreach (var rootMigration in roots)
                await UpMigrationAsync(result, migrationState, rootMigration, cancellationToken);

            return result;
        }
        private async Task UpMigrationAsync(List<IMigrationDefinition> upped, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            if (!handlers.TryGetValue(migration, out IMigrationHandler migrationHandler))
                throw new InvalidOperationException();

            if (!await migrationState.IsAppliedAsync(migration))
            {
                await migrationHandler.UpAsync(cancellationToken);

                await migrationState.SetUpAsync(migration, CancellationToken.None);

                upped.Add(migration);
            }

            if (childs.TryGetValue(migration, out List<MigrationDefinition> childrenMigrations))
            {
                foreach (var childMigration in childrenMigrations)
                    await UpMigrationAsync(upped, migrationState, childMigration, cancellationToken);
            }
        }

        public async Task<List<IMigrationDefinition>> DownAsync(IMigrationState migrationState, CancellationToken cancellationToken)
        {
            var result = new List<IMigrationDefinition>();

            foreach (var rootMigration in roots)
                await DownMigrationAsync(result, migrationState, rootMigration, cancellationToken);

            return result;
        }
        private async Task DownMigrationAsync(List<IMigrationDefinition> downed, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            if (!handlers.TryGetValue(migration, out IMigrationHandler migrationHandler))
                throw new InvalidOperationException();

            if (childs.TryGetValue(migration, out List<MigrationDefinition> childrenMigrations))
            {
                foreach (var childMigration in childrenMigrations)
                    await DownMigrationAsync(downed, migrationState, childMigration, cancellationToken);
            }

            if (await migrationState.IsAppliedAsync(migration))
            {
                await migrationHandler.DownAsync(cancellationToken);

                await migrationState.SetDownAsync(migration, CancellationToken.None);

                downed.Add(migration);
            }
        }

        private bool TryGetByHandlerType(Type handlerType, out MigrationDefinition migrationDefinition)
        {
            if (!migrationTypes.TryGetValue(handlerType, out int index))
            {
                migrationDefinition = null;
                return false;
            }

            migrationDefinition = migrations[index];
            return true;
        }
        private bool TryGetByName(string name, out MigrationDefinition migrationDefinition)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (!migrationNames.TryGetValue(name.ToUpper(), out int index))
            {
                migrationDefinition = null;
                return false;
            }

            migrationDefinition = migrations[index];
            return true;
        }
        private IMigrationHandler CreateMigrationHandler(MigrationDefinition migrationDefinition, IServiceProvider serviceProvider)
        {
            var migrationType = migrationDefinition.HandlerType;
            var migrationConstructor = migrationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public).SingleOrDefault();
            if (migrationConstructor == null)
                throw new InvalidOperationException();

            var constructorParamsInfo = migrationConstructor.GetParameters();
            var constratorParams = new object[constructorParamsInfo.Length];
            var i = 0;
            foreach (var p in constructorParamsInfo)
            {
                constratorParams[i] = serviceProvider.GetRequiredService(p.ParameterType);
                i++;
            }

            return (IMigrationHandler)migrationConstructor.Invoke(constratorParams);
        }
    }
}