namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Implemented by a migration to apply (<see cref="UpAsync"/>) or revert (<see cref="DownAsync"/>) its changes.
    /// </summary>
    public interface IMigrationHandler
    {
        /// <summary>
        /// Applies the migration.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task UpAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Reverts the migration.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        Task DownAsync(CancellationToken cancellationToken = default);
    }
}
