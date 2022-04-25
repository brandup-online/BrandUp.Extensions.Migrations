using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Extensions.Migrations
{
    public interface IMigrationState
    {
        Task<bool> IsAppliedAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
        Task SetUpAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
        Task SetDownAsync(IMigrationDefinition migrationDefinition, CancellationToken cancellationToken = default);
    }
}