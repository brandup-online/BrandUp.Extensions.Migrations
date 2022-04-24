﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrandUp.Extensions.Migrations
{
    public class MigrationLocator : IMigrationLocator
    {
        private readonly Assembly assembly;

        public MigrationLocator(Assembly assembly)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public IEnumerable<MigrationDefinition> GetMigrations()
        {
            var defs = new List<MigrationDefinition>();
            foreach (var type in assembly.GetTypes())
            {
                var migrationAttribute = type.GetCustomAttribute<MigrationAttribute>();
                if (migrationAttribute == null)
                    continue;

                defs.Add(new MigrationDefinition(type, migrationAttribute));
            }
            return defs;
        }
    }
}
