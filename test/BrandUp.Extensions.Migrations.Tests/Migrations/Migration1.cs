using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations.Tests.Migrations
{
    [Migration("1.0.0", "Migration1")]
    public class Migration1 : IMigration
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