namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationHandler
    {
        Task UpAsync(CancellationToken cancellationToken = default);
        Task DownAsync(CancellationToken cancellationToken = default);
    }
}