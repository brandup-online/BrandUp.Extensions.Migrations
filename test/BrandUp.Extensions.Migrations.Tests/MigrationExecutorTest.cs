using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BrandUp.Extensions.Migrations.Tests
{
    public class MigrationExecutorTest
    {
        private readonly IServiceProvider provider;
        private readonly IServiceScope scope;
        private readonly MemoryMigrationStore store;

        public MigrationExecutorTest()
        {
            store = new MemoryMigrationStore();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMigrations(options =>
            {

            });
            services.AddSingleton<IMigrationStore>(store);
            services.AddScoped<Migrations.TestService>();

            provider = services.BuildServiceProvider();
            scope = provider.CreateScope();
        }

        [Fact]
        public async Task UpAsync_First()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();

            var appliedMigrations = await executor.UpAsync();

            Assert.NotEmpty(appliedMigrations);
            Assert.NotEmpty(await store.GetAppliedMigrationsAsync());
        }

        [Fact]
        public async Task UpAsync_Second()
        {
            var locator = scope.ServiceProvider.GetService<IMigrationLocator>();
            var migrations = locator.GetMigrations();
            await store.ApplyMigrationAsync(migrations.OrderBy(it => it.Version).First());

            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();

            var appliedMigrations = await executor.UpAsync();

            Assert.Single(appliedMigrations);
            Assert.Equal(2, (await store.GetAppliedMigrationsAsync()).Count());
        }

        [Fact]
        public async Task DownAsync_Empty()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();

            var appliedMigrations = await executor.DownAsync();

            Assert.Empty(appliedMigrations);
            Assert.Empty(await store.GetAppliedMigrationsAsync());
        }

        [Fact]
        public async Task DownAsync_NotEmpty()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();
            await executor.UpAsync();

            var appliedMigrations = await executor.DownAsync();

            Assert.NotEmpty(appliedMigrations);
            Assert.Empty(await store.GetAppliedMigrationsAsync());
        }

        private class MemoryMigrationStore : IMigrationStore
        {
            private Dictionary<Version, IMigrationVersion> versions = new Dictionary<Version, IMigrationVersion>();

            public Task ApplyMigrationAsync(IMigrationVersion migrationVersion)
            {
                versions.Add(migrationVersion.Version, migrationVersion);

                return Task.CompletedTask;
            }
            public Task CancelMigrationAsync(IMigrationVersion migrationVersion)
            {
                if (!versions.Remove(migrationVersion.Version))
                    throw new Exception();

                return Task.CompletedTask;
            }
            public Task<IEnumerable<IMigrationVersion>> GetAppliedMigrationsAsync()
            {
                return Task.FromResult<IEnumerable<IMigrationVersion>>(versions.Values.OrderBy(it => it.Version));
            }
            public Task<Version> GetCurrentVersionAsync()
            {
                return Task.FromResult(versions.Keys.Max(it => it));
            }
        }
    }
}