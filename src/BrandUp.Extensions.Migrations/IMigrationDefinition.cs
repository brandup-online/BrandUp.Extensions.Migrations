using System;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationDefinition
    {
        string Name { get; }
        Type HandlerType { get; }
        string Description { get; }
    }
}