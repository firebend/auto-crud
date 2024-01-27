using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Web.Interfaces;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.JsonPatch.Extensions;
using Firebend.JsonPatch.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Serialization;

namespace Firebend.AutoCrud.ChangeTracking.Web.Implementations;

public class DefaultChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
{
    private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> _mapper;
    private readonly IJsonPatchGenerator _patchGenerator;

    public DefaultChangeTrackingViewModelMapper(IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> mapper, IJsonPatchGenerator patchGenerator)
    {
        _mapper = mapper;
        _patchGenerator = patchGenerator;
    }

    public async Task<List<ChangeTrackingModel<TKey, TViewModel>>> MapAsync(IEnumerable<ChangeTrackingEntity<TKey, TEntity>> changeTrackingEntities, CancellationToken cancellationToken)
    {
        var trackingEntities = changeTrackingEntities.ToList();
        var models = new List<ChangeTrackingModel<TKey, TViewModel>>(trackingEntities.Count);

        foreach (var changeTrackingEntity in trackingEntities)
        {
            var model = new ChangeTrackingModel<TKey, TViewModel>();

            changeTrackingEntity.CopyPropertiesTo(model, [
                nameof(ChangeTrackingModel<Guid, FooEntity>.Changes),
                nameof(ChangeTrackingModel<Guid, FooEntity>.Entity)
            ]);

            var before = await _mapper.ToAsync(changeTrackingEntity.Entity, cancellationToken);
            model.Entity = before;

            if (changeTrackingEntity.Changes?.HasValues() ?? false)
            {
                var modifiedEntity = changeTrackingEntity.Entity.Clone();
                var patchDoc = new JsonPatchDocument<TEntity>(changeTrackingEntity.Changes, new DefaultContractResolver());
                patchDoc.ApplyTo(modifiedEntity);

                var after = await _mapper.ToAsync(modifiedEntity, cancellationToken);

                var diff = _patchGenerator.Generate(before, after);

                model.Changes = diff.Operations;
            }

            models.Add(model);
        }

        return models;
    }
}
