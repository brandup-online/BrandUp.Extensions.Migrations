namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Base attribute that marks a class as a migration handler. Use <see cref="SetupAttribute"/>
    /// for a root migration or <see cref="UpgradeAttribute"/> for a migration that depends on another one.
    /// </summary>
    public abstract class MigrationAttribute : Attribute
    {
        /// <summary>
        /// Optional human-readable description of the migration.
        /// </summary>
        public string? Description { get; set; }
    }
}
