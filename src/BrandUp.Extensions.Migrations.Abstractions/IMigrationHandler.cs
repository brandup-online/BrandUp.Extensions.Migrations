using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationHandler
    {
        Task UpAsync(CancellationToken cancellationToken = default);
        Task DownAsync(CancellationToken cancellationToken = default);
    }
}