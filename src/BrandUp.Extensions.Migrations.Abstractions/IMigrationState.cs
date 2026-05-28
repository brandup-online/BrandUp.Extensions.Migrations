namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Persists which migrations have been applied. Implementations are responsible for
    /// any concurrency control needed when several application instances run migrations simultaneously.
    /// </summary>
    public interface IMigrationState
    {
        /// <summary>
        /// Returns <c>true</c> if the migration has already been applied.
        /// </summary>
        Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
        /// <summary>
        /// Marks the migration as applied.
        /// </summary>
        Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
        /// <summary>
        /// Marks the migration as reverted.
        /// </summary>
        Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
    }
}
