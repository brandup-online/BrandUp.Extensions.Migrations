using Xunit;

namespace BrandUp.Extensions.Migrations.Tests
{
    public class MigrationDefinitionTest
    {
        [Fact]
        public void Equals_SameHandlerType_AreEqual()
        {
            var a = new MigrationDefinition(typeof(OrderRootA), new SetupAttribute());
            var b = new MigrationDefinition(typeof(OrderRootA), new UpgradeAttribute(typeof(OrderRootB)));

            Assert.True(a.Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equals_DifferentHandlerType_NotEqual()
        {
            var a = new MigrationDefinition(typeof(OrderRootA), new SetupAttribute());
            var b = new MigrationDefinition(typeof(OrderRootB), new SetupAttribute());

            Assert.False(a.Equals(b));
        }

        [Fact]
        public void HashSet_DeduplicatesByHandlerType()
        {
            var set = new HashSet<MigrationDefinition>
            {
                new(typeof(OrderRootA), new SetupAttribute()),
                new(typeof(OrderRootA), new SetupAttribute())
            };

            Assert.Single(set);
        }

        [Fact]
        public void IsRoot_ReflectsAttribute()
        {
            Assert.True(new MigrationDefinition(typeof(OrderRootA), new SetupAttribute()).IsRoot);
            Assert.False(new MigrationDefinition(typeof(OrderChildA), new UpgradeAttribute(typeof(OrderRootA))).IsRoot);
        }

        [Fact]
        public void Ctor_TypeNotImplementingHandler_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => new MigrationDefinition(typeof(UnregisteredDependency), new SetupAttribute()));
        }
    }
}
