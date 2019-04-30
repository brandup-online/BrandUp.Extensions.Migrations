using System;

namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MigrationAttribute : Attribute
    {
        public Version Version { get; }
        public string Description { get; }

        public MigrationAttribute(string version, string description = null)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            Version = Version.Parse(version);
            Description = description;
        }
    }
}