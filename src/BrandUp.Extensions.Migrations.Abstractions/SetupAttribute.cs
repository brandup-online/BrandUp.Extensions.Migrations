using System;

namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SetupAttribute : MigrationAttribute
    {
        public string Name { get; }

        public SetupAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}