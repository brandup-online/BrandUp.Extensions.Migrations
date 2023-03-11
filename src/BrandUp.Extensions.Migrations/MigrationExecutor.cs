using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        MigrationStructure BuildStructure()
        {
            var migrationDefinitions = new HashSet<MigrationDefinition>();
            var assemblies = new List<Assembly>();
            foreach (var assembly in options.Assemblies)
                FindMigrations(migrationDefinitions, assembly, assemblies);

            if (migrationDefinitions.Count == 0)
                return null;

            return new MigrationStructure(migrationDefinitions, logger);
        }
        void FindMigrations(HashSet<MigrationDefinition> migrationDefinitions, Assembly assembly, List<Assembly> assemblies)
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
        readonly List<MigrationDefinition> migrations = new();
        readonly ILogger logger;
        readonly Dictionary<Type, int> migrationTypes = new();
        readonly Dictionary<string, int> migrationNames = new();
        readonly List<MigrationDefinition> roots = new();
        readonly Dictionary<MigrationDefinition, MigrationDefinition> parents = new();
        readonly Dictionary<MigrationDefinition, List<MigrationDefinition>> childs = new();
        readonly Dictionary<MigrationDefinition, IMigrationHandler> handlers = new();

        public MigrationStructure(IEnumerable<MigrationDefinition> migrationDefinitions, ILogger logger)
        {
            this.logger = logger;

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
        async Task UpMigrationAsync(List<IMigrationDefinition> upped, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            if (!handlers.TryGetValue(migration, out IMigrationHandler migrationHandler))
                throw new InvalidOperationException();

            var handlerName = GetHandlerLogName(migrationHandler);

            if (!await migrationState.IsAppliedAsync(migration, cancellationToken))
            {
                logger.LogInformation($"{handlerName}: begin up");

                await migrationHandler.UpAsync(cancellationToken);

                await migrationState.SetUpAsync(migration, cancellationToken);

                logger.LogInformation($"{handlerName}: finish up");

                upped.Add(migration);
            }
            else
                logger.LogInformation($"{handlerName}: already up");

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
        async Task DownMigrationAsync(List<IMigrationDefinition> downed, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            if (!handlers.TryGetValue(migration, out IMigrationHandler migrationHandler))
                throw new InvalidOperationException();

            var handlerName = GetHandlerLogName(migrationHandler);

            if (childs.TryGetValue(migration, out List<MigrationDefinition> childrenMigrations))
            {
                foreach (var childMigration in childrenMigrations)
                    await DownMigrationAsync(downed, migrationState, childMigration, cancellationToken);
            }

            if (await migrationState.IsAppliedAsync(migration, cancellationToken))
            {
                logger.LogInformation($"{handlerName}: begin down");

                await migrationHandler.DownAsync(cancellationToken);

                await migrationState.SetDownAsync(migration, cancellationToken);

                logger.LogInformation($"{handlerName}: finish down");

                downed.Add(migration);
            }
            else
                logger.LogInformation($"{handlerName}: already down");
        }

        bool TryGetByHandlerType(Type handlerType, out MigrationDefinition migrationDefinition)
        {
            if (!migrationTypes.TryGetValue(handlerType, out int index))
            {
                migrationDefinition = null;
                return false;
            }

            migrationDefinition = migrations[index];
            return true;
        }
        bool TryGetByName(string name, out MigrationDefinition migrationDefinition)
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
        IMigrationHandler CreateMigrationHandler(MigrationDefinition migrationDefinition, IServiceProvider serviceProvider)
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

        static string GetHandlerLogName(IMigrationHandler migrationHandler)
        {
            return $"{migrationHandler.GetType().Assembly.FullName}, {migrationHandler.GetType().FullName}";
        }
    }
}