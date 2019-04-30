using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationStore
    {
        Task<Version> GetCurrentVersionAsync();
        Task ApplyMigrationAsync(IMigrationVersion migrationVersion);
        Task CancelMigrationAsync(IMigrationVersion migrationVersion);
        Task<IEnumerable<IMigrationVersion>> GetAppliedMigrationsAsync();
    }
}