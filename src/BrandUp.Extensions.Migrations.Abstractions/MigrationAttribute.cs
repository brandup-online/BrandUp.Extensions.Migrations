using System;

namespace BrandUp.Extensions.Migrations
{
    public abstract class MigrationAttribute : Attribute
    {
        public string Description { get; set; }
    }
}