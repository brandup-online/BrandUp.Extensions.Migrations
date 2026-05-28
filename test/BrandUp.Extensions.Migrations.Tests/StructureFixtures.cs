using System.Reflection;

namespace BrandUp.Extensions.Migrations.Tests
{
    // Handlers below intentionally carry NO migration attribute, so the real MigrationLocator
    // ignores them during assembly scans. Migration structure is supplied explicitly via FakeLocator.

    sealed class FakeLocator(params MigrationDefinition[] definitions) : IMigrationLocator
    {
        public IEnumerable<MigrationDefinition> FindMigrations(Assembly assembly) => definitions;
    }

    sealed class Recorder
    {
        public List<string> Up { get; } = [];
        public List<string> Down { get; } = [];
    }

    sealed class MemoryState : IMigrationState
    {
        public HashSet<string> Applied { get; } = new(StringComparer.Ordinal);

        public Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
            => Task.FromResult(Applied.Contains(migrationDefinition.Name));

        public Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
        {
            Applied.Add(migrationDefinition.Name);
            return Task.CompletedTask;
        }

        public Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default)
        {
            Applied.Remove(migrationDefinition.Name);
            return Task.CompletedTask;
        }
    }

    abstract class RecordingHandler(Recorder recorder) : IMigrationHandler
    {
        public Task UpAsync(CancellationToken cancellationToken = default)
        {
            recorder.Up.Add(GetType().Name);
            return Task.CompletedTask;
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            recorder.Down.Add(GetType().Name);
            return Task.CompletedTask;
        }
    }

    sealed class OrderRootA(Recorder recorder) : RecordingHandler(recorder);
    sealed class OrderRootB(Recorder recorder) : RecordingHandler(recorder);
    sealed class OrderChildA(Recorder recorder) : RecordingHandler(recorder);
    sealed class OrderChildB(Recorder recorder) : RecordingHandler(recorder);
    sealed class OrderChildC(Recorder recorder) : RecordingHandler(recorder);

    sealed class CycleA : IMigrationHandler
    {
        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    sealed class CycleB : IMigrationHandler
    {
        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    sealed class Orphan : IMigrationHandler
    {
        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    sealed class UnregisteredParent : IMigrationHandler
    {
        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    sealed class UnregisteredDependency { }

    sealed class LazyHandler : IMigrationHandler
    {
        public LazyHandler(UnregisteredDependency dependency) => _ = dependency;

        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    sealed class MultiCtorHandler : IMigrationHandler
    {
        public MultiCtorHandler() { }
        public MultiCtorHandler(Recorder recorder) => _ = recorder;

        public Task UpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DownAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
