using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.ChangeTracking.Web
{
    internal static class ChangeTracingViewModelCaches
    {
        public static readonly string[] MapperIgnores = {
            nameof(ChangeTrackingModel<Guid, FooEntity>.Changes),
            nameof(ChangeTrackingModel<Guid, FooEntity>.Entity)
        };
    }
    public class ChangeTrackingViewModel<TKey, TEntity, TVersion, TViewModel> : ChangeTrackingModel<TKey, TViewModel>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        where TViewModel : class
    {
        public async Task<ChangeTrackingViewModel<TKey, TEntity, TVersion, TViewModel>> MapAsync(ChangeTrackingEntity<TKey, TEntity> changeTrackingEntity,
            IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> mapper,
            CancellationToken cancellationToken = default)
        {
            changeTrackingEntity.CopyPropertiesTo(this, ChangeTracingViewModelCaches.MapperIgnores);

            var mapped = await mapper
                .ToAsync(changeTrackingEntity.Entity, cancellationToken)
                .ConfigureAwait(false);

            Entity = mapped;

            Changes = changeTrackingEntity
                .Changes
                .NullCheck()
                .Where(x => x != null)
                .Select(x => new Operation<TViewModel>(x.op, x.path, x.from, x.value))
                .ToList();

            return this;
        }
    }
}
