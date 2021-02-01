using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsUpdateService<TRootEntityKey> : IDisposable
        where TRootEntityKey : struct
    {

        Task<CustomFieldsEntity<TRootEntityKey>> UpdateAsync(TRootEntityKey rootEntityKey,
            CustomFieldsEntity<TRootEntityKey> entity,
            CancellationToken cancellationToken = default);

        Task<CustomFieldsEntity<TRootEntityKey>> PatchAsync(TRootEntityKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TRootEntityKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default);
    }
}
