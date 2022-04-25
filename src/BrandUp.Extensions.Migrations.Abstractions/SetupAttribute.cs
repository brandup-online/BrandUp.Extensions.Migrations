using System;

namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SetupAttribute : MigrationAttribute { }
}