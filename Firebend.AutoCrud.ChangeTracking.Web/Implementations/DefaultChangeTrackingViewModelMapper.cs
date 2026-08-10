using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Extensions;
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

// A thin closure of the generic 6-arg version over the default row type and DTO, kept as its own
// named class since it predates custom row types and is part of the public API.
public class DefaultChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel> :
    DefaultChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel, ChangeTrackingEntity<TKey, TEntity>, ChangeTrackingModel<TKey, TViewModel>>,
    IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
{
    public DefaultChangeTrackingViewModelMapper(IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> mapper, IJsonPatchGenerator patchGenerator)
        : base(mapper, patchGenerator)
    {
    }
}

/// <summary>
/// Default implementation of <see cref="IChangeTrackingViewModelMapper{TKey,TEntity,TVersion,TViewModel,TChangeTrackingEntity,TChangeTrackingViewModel}"/>.
/// Copies matching properties from <typeparamref name="TChangeTrackingEntity"/> onto <typeparamref name="TChangeTrackingViewModel"/>
/// by name, so any extra columns a consumer added to their custom row type carry through automatically as long as
/// their DTO declares a property with the same name.
/// </summary>
public class DefaultChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel, TChangeTrackingEntity, TChangeTrackingViewModel>
    : IChangeTrackingViewModelMapper<TKey, TEntity, TVersion, TViewModel, TChangeTrackingEntity, TChangeTrackingViewModel>
    where TViewModel : class
    where TEntity : class, IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TChangeTrackingEntity : ChangeTrackingEntity<TKey, TEntity>
    where TChangeTrackingViewModel : ChangeTrackingModel<TKey, TViewModel>, new()
{
    private readonly IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> _mapper;
    private readonly IJsonPatchGenerator _patchGenerator;

    public DefaultChangeTrackingViewModelMapper(IReadViewModelMapper<TKey, TEntity, TVersion, TViewModel> mapper, IJsonPatchGenerator patchGenerator)
    {
        _mapper = mapper;
        _patchGenerator = patchGenerator;
    }

    public async Task<List<TChangeTrackingViewModel>> MapAsync(IEnumerable<TChangeTrackingEntity> changeTrackingEntities, CancellationToken cancellationToken)
    {
        var trackingEntities = changeTrackingEntities.ToList();
        var models = new List<TChangeTrackingViewModel>(trackingEntities.Count);

        foreach (var changeTrackingEntity in trackingEntities)
        {
            var model = new TChangeTrackingViewModel();

            changeTrackingEntity.CopyPropertiesTo(model, [
                nameof(ChangeTrackingModel<Guid, FooEntity>.Changes),
                nameof(ChangeTrackingModel<Guid, FooEntity>.Entity)
            ]);

            var before = await _mapper.ToAsync(changeTrackingEntity.Entity, cancellationToken);
            model.Entity = before;

            if (changeTrackingEntity.Changes?.HasValues() ?? false)
            {
                var modifiedEntity = changeTrackingEntity.Entity.Clone();
                var patchDoc = new JsonPatchDocument(changeTrackingEntity.Changes, new DefaultContractResolver());
                patchDoc.ApplyTo(modifiedEntity);

                var after = await _mapper.ToAsync(modifiedEntity, cancellationToken);

                var diff = _patchGenerator.Generate(before, after);

                model.Changes = diff.Operations.ToNonGenericOperations();
            }

            models.Add(model);
        }

        return models;
    }
}
