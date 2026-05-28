namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Marks a migration handler that must be applied after the migration specified by <see cref="AfterType"/>.
    /// </summary>
    /// <param name="migrationAfterType">The handler type this migration depends on.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UpgradeAttribute(Type migrationAfterType) : MigrationAttribute
    {
        /// <summary>
        /// The migration handler type that must be applied before this one.
        /// </summary>
        public Type AfterType { get; } = migrationAfterType ?? throw new ArgumentNullException(nameof(migrationAfterType));
    }
}
