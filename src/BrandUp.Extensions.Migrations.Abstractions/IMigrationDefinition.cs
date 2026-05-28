namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Describes a single migration discovered in an assembly.
    /// </summary>
    public interface IMigrationDefinition
    {
        /// <summary>
        /// Unique name of the migration (the full name of the handler type).
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Type that implements <see cref="IMigrationHandler"/> for this migration.
        /// </summary>
        Type HandlerType { get; }
        /// <summary>
        /// Optional human-readable description provided via the migration attribute.
        /// </summary>
        string? Description { get; }
    }
}
