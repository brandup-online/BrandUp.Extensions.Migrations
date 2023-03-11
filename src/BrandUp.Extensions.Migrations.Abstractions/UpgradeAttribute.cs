namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UpgradeAttribute : MigrationAttribute
    {
        public Type AfterType { get; }

        public UpgradeAttribute(Type migrationAfterType)
        {
            AfterType = migrationAfterType ?? throw new ArgumentNullException(nameof(migrationAfterType));
        }
    }
}