using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsCreateService<TRootEntityKey> : IDisposable
        where TRootEntityKey : struct
    {

        Task<CustomFieldsEntity<TRootEntityKey>> CreateAsync(TRootEntityKey rootEntityKey,
            CustomFieldsEntity<TRootEntityKey> entity,
            CancellationToken cancellationToken = default);
    }
}
