# BrandUp.Extensions.Migrations

## Define migrations
```
[Migration("1.0.0", "Migration1")]
public class Migration1 : IMigration
{
	readpnly IDbContext dbContext;

	public Migration1(IDbContext dbContext)
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

services.AddScoped<IDbContext, DbContext>();
services.AddMigrations(typeof(Migration1).Assembly);
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