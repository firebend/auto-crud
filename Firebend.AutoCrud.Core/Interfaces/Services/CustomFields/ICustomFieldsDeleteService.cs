using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsDeleteService<TRootEntityKey> : IDisposable
        where TRootEntityKey : struct
    {
        Task<CustomFieldsEntity<TRootEntityKey>> DeleteAsync(TRootEntityKey rootEntityKey, Guid key, CancellationToken cancellationToken = default);
    }
}
