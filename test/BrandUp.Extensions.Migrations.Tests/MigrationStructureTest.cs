using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BrandUp.Extensions.Migrations.Tests
{
    public class MigrationStructureTest
    {
        static (MigrationExecutor executor, Recorder recorder, MemoryState state) Build(params MigrationDefinition[] definitions)
        {
            var recorder = new Recorder();
            var state = new MemoryState();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(recorder);
            services.AddSingleton<IMigrationState>(state);
            services.AddMigrations(options => options.AddAssembly(typeof(FakeLocator).Assembly));
            services.AddSingleton<IMigrationLocator>(new FakeLocator(definitions));

            var provider = services.BuildServiceProvider();

            return (provider.GetRequiredService<MigrationExecutor>(), recorder, state);
        }

        [Fact]
        public async Task UpAsync_AppliesParentsBeforeChildrenInDeterministicOrder()
        {
            var (executor, recorder, _) = Build(
                new MigrationDefinition(typeof(OrderRootB), new SetupAttribute()),
                new MigrationDefinition(typeof(OrderChildC), new UpgradeAttribute(typeof(OrderRootA))),
                new MigrationDefinition(typeof(OrderRootA), new SetupAttribute()),
                new MigrationDefinition(typeof(OrderChildA), new UpgradeAttribute(typeof(OrderRootA))),
                new MigrationDefinition(typeof(OrderChildB), new UpgradeAttribute(typeof(OrderRootA))));

            var applied = await executor.UpAsync(TestContext.Current.CancellationToken);

            Assert.Equal(["OrderRootA", "OrderChildA", "OrderChildB", "OrderChildC", "OrderRootB"], recorder.Up);
            Assert.Equal(5, applied.Count);
        }

        [Fact]
        public async Task DownAsync_RevertsChildrenBeforeParents()
        {
            var (executor, recorder, _) = Build(
                new MigrationDefinition(typeof(OrderRootA), new SetupAttribute()),
                new MigrationDefinition(typeof(OrderChildA), new UpgradeAttribute(typeof(OrderRootA))),
                new MigrationDefinition(typeof(OrderChildB), new UpgradeAttribute(typeof(OrderRootA))));
            await executor.UpAsync(TestContext.Current.CancellationToken);

            var downed = await executor.DownAsync(TestContext.Current.CancellationToken);

            Assert.Equal(["OrderChildA", "OrderChildB", "OrderRootA"], recorder.Down);
            Assert.Equal(3, downed.Count);
        }

        [Fact]
        public async Task UpAsync_CyclicDependencies_Throws()
        {
            var (executor, _, _) = Build(
                new MigrationDefinition(typeof(CycleA), new UpgradeAttribute(typeof(CycleB))),
                new MigrationDefinition(typeof(CycleB), new UpgradeAttribute(typeof(CycleA))));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.UpAsync(TestContext.Current.CancellationToken));
            Assert.Contains("Circular or unreachable", ex.Message);
        }

        [Fact]
        public async Task UpAsync_MissingParent_Throws()
        {
            var (executor, _, _) = Build(
                new MigrationDefinition(typeof(Orphan), new UpgradeAttribute(typeof(UnregisteredParent))));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.UpAsync(TestContext.Current.CancellationToken));
            Assert.Contains("not a registered migration", ex.Message);
        }

        [Fact]
        public async Task UpAsync_AlreadyApplied_DoesNotCreateHandler()
        {
            var (executor, _, state) = Build(new MigrationDefinition(typeof(LazyHandler), new SetupAttribute()));
            state.Applied.Add(typeof(LazyHandler).FullName!);

            var applied = await executor.UpAsync(TestContext.Current.CancellationToken);

            // No exception, even though LazyHandler depends on an unregistered service:
            // the handler is never instantiated because the migration is already applied.
            Assert.Empty(applied);
        }

        [Fact]
        public async Task UpAsync_NotApplied_InstantiatesHandlerAndResolvesDependencies()
        {
            var (executor, _, _) = Build(new MigrationDefinition(typeof(LazyHandler), new SetupAttribute()));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.UpAsync(TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task UpAsync_HandlerWithMultipleConstructors_Throws()
        {
            var (executor, _, _) = Build(new MigrationDefinition(typeof(MultiCtorHandler), new SetupAttribute()));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.UpAsync(TestContext.Current.CancellationToken));
            Assert.Contains("exactly one public constructor", ex.Message);
        }
    }
}
