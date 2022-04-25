using BrandUp.Extensions.Migrations.Tests.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
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
                options.AddAssembly(typeof(Migration1).Assembly);
            });
            services.AddSingleton<IMigrationState>(store);
            services.AddScoped<TestService>();

            provider = services.BuildServiceProvider();
            scope = provider.CreateScope();
        }

        [Fact]
        public async Task UpAsync_First()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();

            var appliedMigrations = await executor.UpAsync();

            Assert.Equal(2, appliedMigrations.Count);
            Assert.Equal(2, store.Names.Count);
            Assert.Equal("brandup.extensions.migrations.tests.migrations.migration1", store.Names[0].ToLower());
            Assert.Equal("brandup.extensions.migrations.tests.migrations.migration2", store.Names[1].ToLower());
        }

        [Fact]
        public async Task UpAsync_Second()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();

            var appliedMigrations = await executor.UpAsync();
            Assert.Equal(2, appliedMigrations.Count);

            appliedMigrations = await executor.UpAsync();

            Assert.Empty(appliedMigrations);
            Assert.Equal(2, store.Names.Count);
        }

        [Fact]
        public async Task DownAsync_First()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();
            await executor.UpAsync();

            var downedMigrations = await executor.DownAsync();

            Assert.Equal(2, downedMigrations.Count);
            Assert.Empty(store.Names);
            Assert.Equal("brandup.extensions.migrations.tests.migrations.migration2", downedMigrations[0].Name.ToLower());
            Assert.Equal("brandup.extensions.migrations.tests.migrations.migration1", downedMigrations[1].Name.ToLower());
        }

        [Fact]
        public async Task DownAsync_Second()
        {
            var executor = scope.ServiceProvider.GetService<MigrationExecutor>();
            await executor.UpAsync();
            var downedMigrations = await executor.DownAsync();
            Assert.Equal(2, downedMigrations.Count);

            downedMigrations = await executor.DownAsync();

            Assert.Empty(downedMigrations);
            Assert.Empty(store.Names);
        }

        private class MemoryMigrationStore : IMigrationState
        {
            readonly List<string> names = new List<string>();

            public List<string> Names => names;

            public Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(names.Contains(migrationDefinition.Name.ToUpper()));
            }

            public Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
            {
                names.Add(migrationDefinition.Name.ToUpper());

                return Task.CompletedTask;
            }

            public Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
            {
                if (!names.Remove(migrationDefinition.Name.ToUpper()))
                    throw new InvalidOperationException();

                return Task.CompletedTask;
            }
        }
    }
}