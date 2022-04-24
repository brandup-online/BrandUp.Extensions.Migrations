using System;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationDefinition : IMigrationVersion, IComparable<MigrationDefinition>
    {
        readonly MigrationAttribute attribute;

        public Guid Id { get; } = Guid.NewGuid();
        public Version Version { get; }
        public string Description { get; }
        public Type MigrationType { get; }

        public MigrationDefinition(Type migrationType, MigrationAttribute migrationAttribute)
        {
            MigrationType = migrationType ?? throw new ArgumentNullException(nameof(migrationType));
            attribute = migrationAttribute ?? throw new ArgumentNullException(nameof(migrationAttribute));
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