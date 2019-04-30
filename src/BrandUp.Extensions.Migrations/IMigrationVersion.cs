using System;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationVersion
    {
        Guid Id { get; }
        Version Version { get; }
        string Description { get; }
    }
}