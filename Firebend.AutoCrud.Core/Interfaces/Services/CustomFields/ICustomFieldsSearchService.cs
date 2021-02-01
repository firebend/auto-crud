using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsSearchService<TRootEntityKey> : IDisposable
        where TRootEntityKey : struct
    {
        Task<EntityPagedResponse<CustomFieldsEntity<TRootEntityKey>>> SearchAsync(
            string key,
            string value,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default);
    }
}
