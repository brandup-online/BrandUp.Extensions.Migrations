using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Discovers migrations in the configured assemblies and applies or reverts them in dependency order.
    /// </summary>
    /// <param name="options">Migration options describing which assemblies to scan.</param>
    /// <param name="migrationLocator">Locator used to discover migrations within an assembly.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="serviceProvider">Root service provider used to create a scope for each run.</param>
    public class MigrationExecutor(IOptions<MigrationOptions> options, IMigrationLocator migrationLocator, ILogger<MigrationExecutor> logger, IServiceProvider serviceProvider) : IMigrationExecutor
    {
        readonly MigrationOptions options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        readonly IMigrationLocator migrationLocator = migrationLocator ?? throw new ArgumentNullException(nameof(migrationLocator));
        readonly ILogger<MigrationExecutor> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        readonly IServiceProvider serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        /// <inheritdoc />
        public async Task<List<IMigrationDefinition>> UpAsync(CancellationToken cancellationToken = default)
        {
            var structure = BuildStructure();
            if (structure == null)
            {
                logger.LogInformation("No migrations found.");
                return [];
            }

            using var scope = serviceProvider.CreateScope();
            var migrationState = scope.ServiceProvider.GetRequiredService<IMigrationState>();

            return await structure.UpAsync(scope.ServiceProvider, migrationState, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<List<IMigrationDefinition>> DownAsync(CancellationToken cancellationToken = default)
        {
            var structure = BuildStructure();
            if (structure == null)
            {
                logger.LogInformation("No migrations found.");
                return [];
            }

            using var scope = serviceProvider.CreateScope();
            var migrationState = scope.ServiceProvider.GetRequiredService<IMigrationState>();

            return await structure.DownAsync(scope.ServiceProvider, migrationState, cancellationToken).ConfigureAwait(false);
        }

        MigrationStructure? BuildStructure()
        {
            var migrationDefinitions = new HashSet<MigrationDefinition>();
            var scannedAssemblies = new HashSet<Assembly>();
            foreach (var assembly in options.Assemblies)
                FindMigrations(migrationDefinitions, assembly, scannedAssemblies);

            if (migrationDefinitions.Count == 0)
                return null;

            return new MigrationStructure(migrationDefinitions, logger);
        }

        void FindMigrations(HashSet<MigrationDefinition> migrationDefinitions, Assembly assembly, HashSet<Assembly> scannedAssemblies)
        {
            if (!scannedAssemblies.Add(assembly))
                return;

            foreach (var m in migrationLocator.FindMigrations(assembly))
            {
                if (!migrationDefinitions.Add(m))
                    continue;

                if (!m.IsRoot)
                    FindMigrations(migrationDefinitions, m.ParentHandlerType!.Assembly, scannedAssemblies);
            }
        }
    }

    class MigrationStructure
    {
        readonly List<MigrationDefinition> migrations = [];
        readonly ILogger logger;
        readonly Dictionary<Type, MigrationDefinition> migrationsByType = [];
        readonly List<MigrationDefinition> roots = [];
        readonly Dictionary<MigrationDefinition, List<MigrationDefinition>> children = [];

        public MigrationStructure(IEnumerable<MigrationDefinition> migrationDefinitions, ILogger logger)
        {
            this.logger = logger;

            foreach (var m in migrationDefinitions)
            {
                migrations.Add(m);
                migrationsByType.Add(m.HandlerType, m);

                if (m.IsRoot)
                    roots.Add(m);
            }

            foreach (var migration in migrations)
            {
                if (migration.IsRoot)
                    continue;

                if (!migrationsByType.TryGetValue(migration.ParentHandlerType!, out var parentMigration))
                    throw new InvalidOperationException($"Migration \"{migration.Name}\" must be applied after \"{migration.ParentHandlerType}\", which is not a registered migration.");

                if (!children.TryGetValue(parentMigration, out var childMigrations))
                    children.Add(parentMigration, childMigrations = []);

                childMigrations.Add(migration);
            }

            roots.Sort(CompareByName);
            foreach (var childList in children.Values)
                childList.Sort(CompareByName);

            EnsureReachable();
        }

        public async Task<List<IMigrationDefinition>> UpAsync(IServiceProvider serviceProvider, IMigrationState migrationState, CancellationToken cancellationToken)
        {
            var result = new List<IMigrationDefinition>();

            foreach (var rootMigration in roots)
                await UpMigrationAsync(serviceProvider, result, migrationState, rootMigration, cancellationToken).ConfigureAwait(false);

            return result;
        }

        async Task UpMigrationAsync(IServiceProvider serviceProvider, List<IMigrationDefinition> upped, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            var logName = GetMigrationLogName(migration);

            if (!await migrationState.IsAppliedAsync(migration, cancellationToken).ConfigureAwait(false))
            {
                logger.LogInformation("{Migration}: begin up", logName);

                var migrationHandler = CreateMigrationHandler(migration, serviceProvider);
                await migrationHandler.UpAsync(cancellationToken).ConfigureAwait(false);

                await migrationState.SetUpAsync(migration, cancellationToken).ConfigureAwait(false);

                logger.LogInformation("{Migration}: finish up", logName);

                upped.Add(migration);
            }
            else
                logger.LogInformation("{Migration}: already up", logName);

            if (children.TryGetValue(migration, out var childrenMigrations))
            {
                foreach (var childMigration in childrenMigrations)
                    await UpMigrationAsync(serviceProvider, upped, migrationState, childMigration, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<List<IMigrationDefinition>> DownAsync(IServiceProvider serviceProvider, IMigrationState migrationState, CancellationToken cancellationToken)
        {
            var result = new List<IMigrationDefinition>();

            foreach (var rootMigration in roots)
                await DownMigrationAsync(serviceProvider, result, migrationState, rootMigration, cancellationToken).ConfigureAwait(false);

            return result;
        }

        async Task DownMigrationAsync(IServiceProvider serviceProvider, List<IMigrationDefinition> downed, IMigrationState migrationState, MigrationDefinition migration, CancellationToken cancellationToken)
        {
            if (children.TryGetValue(migration, out var childrenMigrations))
            {
                foreach (var childMigration in childrenMigrations)
                    await DownMigrationAsync(serviceProvider, downed, migrationState, childMigration, cancellationToken).ConfigureAwait(false);
            }

            var logName = GetMigrationLogName(migration);

            if (await migrationState.IsAppliedAsync(migration, cancellationToken).ConfigureAwait(false))
            {
                logger.LogInformation("{Migration}: begin down", logName);

                var migrationHandler = CreateMigrationHandler(migration, serviceProvider);
                await migrationHandler.DownAsync(cancellationToken).ConfigureAwait(false);

                await migrationState.SetDownAsync(migration, cancellationToken).ConfigureAwait(false);

                logger.LogInformation("{Migration}: finish down", logName);

                downed.Add(migration);
            }
            else
                logger.LogInformation("{Migration}: already down", logName);
        }

        void EnsureReachable()
        {
            var visited = new HashSet<MigrationDefinition>();
            var stack = new Stack<MigrationDefinition>(roots);
            while (stack.Count > 0)
            {
                var migration = stack.Pop();
                if (!visited.Add(migration))
                    continue;

                if (children.TryGetValue(migration, out var childList))
                {
                    foreach (var child in childList)
                        stack.Push(child);
                }
            }

            if (visited.Count != migrations.Count)
            {
                var unreachable = migrations.Where(m => !visited.Contains(m)).Select(m => m.Name);
                throw new InvalidOperationException($"Circular or unreachable migration dependencies detected: {string.Join(", ", unreachable)}.");
            }
        }

        static int CompareByName(MigrationDefinition x, MigrationDefinition y)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }

        static IMigrationHandler CreateMigrationHandler(MigrationDefinition migrationDefinition, IServiceProvider serviceProvider)
        {
            var migrationType = migrationDefinition.HandlerType;
            var constructors = migrationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            if (constructors.Length != 1)
                throw new InvalidOperationException($"Migration handler \"{migrationType.FullName}\" must have exactly one public constructor, but {constructors.Length} were found.");

            var migrationConstructor = constructors[0];
            var constructorParamsInfo = migrationConstructor.GetParameters();
            var constructorParams = new object[constructorParamsInfo.Length];
            for (var i = 0; i < constructorParamsInfo.Length; i++)
                constructorParams[i] = serviceProvider.GetRequiredService(constructorParamsInfo[i].ParameterType);

            return (IMigrationHandler)migrationConstructor.Invoke(constructorParams);
        }

        static string GetMigrationLogName(MigrationDefinition migration)
        {
            return $"{migration.HandlerType.Assembly.FullName}, {migration.HandlerType.FullName}";
        }
    }
}
