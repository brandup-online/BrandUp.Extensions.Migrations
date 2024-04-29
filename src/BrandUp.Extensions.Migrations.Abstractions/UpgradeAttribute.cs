namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UpgradeAttribute(Type migrationAfterType) : MigrationAttribute
    {
        public Type AfterType { get; } = migrationAfterType ?? throw new ArgumentNullException(nameof(migrationAfterType));
    }
}