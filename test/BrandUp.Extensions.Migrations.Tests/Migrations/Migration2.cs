using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations.Tests.Migrations
{
    [Upgrade(After = typeof(Migration1))]
    public class Migration2 : IMigration
    {
        public Task UpAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DownAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
