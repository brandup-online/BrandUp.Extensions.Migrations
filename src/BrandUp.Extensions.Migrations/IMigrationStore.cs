using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationStore
    {
        Task<Version> GetCurrentVersionAsync();
        Task ApplyMigration(IMigrationVersion migrationVersion);
        Task CancelMigration(IMigrationVersion migrationVersion);
        Task<IEnumerable<IMigrationVersion>> GetAppliedMigrationsAsync();
    }
}