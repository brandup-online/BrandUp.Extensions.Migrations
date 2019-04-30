using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigration
    {
        Task UpAsync(CancellationToken cancellationToken = default);
        Task DownAsync(CancellationToken cancellationToken = default);
    }
}