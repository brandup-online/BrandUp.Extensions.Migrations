using BrandUp.Extensions.Migrations.Tests.Migrations;
using System.Linq;
using Xunit;

namespace BrandUp.Extensions.Migrations.Tests
{
    public class MigrationLocatorTest
    {
        [Fact]
        public void FindMigrations()
        {
            var locator = new MigrationLocator();

            var migrations = locator.FindMigrations(typeof(Migration1).Assembly);

            Assert.NotEmpty(migrations);
            Assert.Equal(2, migrations.Count());
        }
    }
}