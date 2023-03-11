namespace BrandUp.Extensions.Migrations.Tests.Migrations
{
    [Setup]
    public class Migration1 : IMigrationHandler
    {
        private readonly TestService service;

        public Migration1(TestService service)
        {
            this.service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        public Task UpAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestService
    {

    }
}