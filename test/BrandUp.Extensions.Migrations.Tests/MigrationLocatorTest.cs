using System;
using System.Linq;
using Xunit;

namespace BrandUp.Extensions.Migrations.Tests
{
    public class MigrationLocatorTest
    {
        private MigrationLocator locator;

        public MigrationLocatorTest()
        {
            locator = new MigrationLocator(typeof(MigrationLocatorTest).Assembly);
        }

        [Fact]
        public void GetMigrations_All()
        {
            var migrations = locator.GetMigrations(new Version("0.0.0"));

            Assert.NotEmpty(migrations);
            Assert.Equal(2, migrations.Count());
        }

        [Fact]
        public void GetMigrations_After()
        {
            var migrations = locator.GetMigrations(new Version("1.0.0"));

            Assert.NotEmpty(migrations);
            Assert.Single(migrations);
        }
    }
}