using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityTransactionOutboxEnrollment
{
    public Task ActAsync(CancellationToken cancellationToken);
}
