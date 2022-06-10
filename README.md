# BrandUp.Extensions.Migrations

## Installation

NuGet-package: [https://www.nuget.org/packages/BrandUp.Extensions.Migrations/](https://www.nuget.org/packages/BrandUp.Extensions.Migrations/)

## Architecture

	ExampleLib1 assembly
		ExampleLib1.SetupMigration
		ExampleLib1.Update1Migration
		ExampleLib1.Update2Migration
	ExampleLib2 assembly
		ExampleLib2.SetupMigration
		ExampleLib2.Update1Migration
		ExampleLib2.Update2Migration
	MyApp assembly
		MyApp.SetupMigration
		MyApp.CreateIndexesMigration
		MyApp.DefaultUsersMigration

	options.AddAssembly(typeof(ExampleLib1.SetupMigration).Assembly);
	options.AddAssembly(typeof(ExampleLib2.SetupMigration).Assembly);
	options.AddAssembly(typeof(MyApp.SetupMigration).Assembly);

	migrationExecutor.UpAsync()

## Define migrations

```
[Setup]
public class SetupMigration : IMigrationHandler
{
	readpnly IDbContext dbContext;

	public SetupMigration(IDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	public Task UpAsync(CancellationToken cancellationToken = default)
	{
		dbContext.Create();		

		return Task.CompletedTask;
	}

	public Task DownAsync(CancellationToken cancellationToken = default)
	{
		dbContext.Delete();		

		return Task.CompletedTask;
	}
}

[Upgrade(typeof(SetupMigration))]
public class Update1Migration : IMigrationHandler
{
	public Task UpAsync(CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}

	public Task DownAsync(CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}
}

services.AddScoped<IDbContext, DbContext>();
services.AddMigrations(options => {
	options.AddAssembly(typeof(SetupMigration).Assembly);
});
```

## Up migrations

```
var migrationExecutor = serviceProvider.GetService<MigrationExecutor>();
migrationExecutor.UpAsync();
```

## Down migrations

```
var migrationExecutor = serviceProvider.GetService<MigrationExecutor>();
migrationExecutor.DownAsync();
```

## Store migration state

```
public interface IMigrationState
{
	Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
	Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
	Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
}
```