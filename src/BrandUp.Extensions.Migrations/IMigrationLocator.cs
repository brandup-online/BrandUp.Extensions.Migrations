using System;
using System.Collections.Generic;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationLocator
    {
        IEnumerable<MigrationDefinition> GetMigrations(Version after);
    }
}