namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Applies or reverts the migrations discovered in the configured assemblies.
    /// </summary>
    public interface IMigrationExecutor
    {
        /// <summary>
        /// Applies all not-yet-applied migrations, parents before children.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The migrations that were applied during this call.</returns>
        Task<List<IMigrationDefinition>> UpAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reverts all applied migrations, children before parents.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The migrations that were reverted during this call.</returns>
        Task<List<IMigrationDefinition>> DownAsync(CancellationToken cancellationToken = default);
    }
}
