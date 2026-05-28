namespace BrandUp.Extensions.Migrations
{
    /// <summary>
    /// Marks a migration handler as a root migration (a starting point with no predecessor).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SetupAttribute : MigrationAttribute { }
}
