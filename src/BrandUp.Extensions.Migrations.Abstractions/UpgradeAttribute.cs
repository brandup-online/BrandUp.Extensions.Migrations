using System;

namespace BrandUp.Extensions.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UpgradeAttribute : MigrationAttribute
    {
        public Type After { get; set; }
        public Version Version { get; }
        public string Description { get; set; }

        public UpgradeAttribute(string version = null)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            Version = Version.Parse(version);
        }
    }

    public abstract class MigrationAttribute : Attribute { }
}