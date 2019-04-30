using System;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationDefinition : IMigrationVersion, IComparable<MigrationDefinition>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Version Version { get; }
        public string Description { get; }
        public Type MigrationType { get; }

        public MigrationDefinition(Version version, Type migrationType, string description = null)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            MigrationType = migrationType ?? throw new ArgumentNullException(nameof(migrationType));
            Description = description;
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }
        public override string ToString()
        {
            return $"[{Version}] {Description}";
        }

        #region IComparable members

        public int CompareTo(MigrationDefinition other)
        {
            return Version.CompareTo(other.Version);
        }

        #endregion
    }
}