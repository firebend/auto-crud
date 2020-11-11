namespace Firebend.AutoCrud.ChangeTracking.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoCrud.Web.Interfaces;
    using Core.Extensions;
    using Core.Interfaces.Models;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Models;

    public class ChangeTrackingViewModel<TKey, TEntity, TViewModel> : ChangeTrackingModel<TKey, TViewModel>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TViewModel : class
    {
        public async Task<ChangeTrackingViewModel<TKey, TEntity, TViewModel>> MapAsync(ChangeTrackingEntity<TKey, TEntity> changeTrackingEntity,
            IViewModelMapper<TKey, TEntity, TViewModel> mapper,
            CancellationToken cancellationToken = default)
        {
            changeTrackingEntity.CopyPropertiesTo(this,
                nameof(ChangeTrackingModel<Guid, FooEntity>.Changes),
                nameof(ChangeTrackingModel<Guid, FooEntity>.Entity));

            var mapped = await mapper
                .ToAsync(changeTrackingEntity.Entity, cancellationToken)
                .ConfigureAwait(false);

            Entity = mapped;

            Changes = changeTrackingEntity
                .Changes
                .Select(x => new Operation<TViewModel>(x.op, x.path, x.from, x.value))
                .ToList();

            return this;
        }
    }
}
