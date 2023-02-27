using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web;

public static class ControllerExtensions
{

    public static async Task<TEntity> ValidateModel<TKey, TEntity, TVersion, TCreateViewModel>(this AbstractEntityControllerBase<TVersion> controller,
        TCreateViewModel body,
        IViewModelMapper<TKey, TEntity, TVersion, TCreateViewModel> mapper,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IApiVersion
        where TCreateViewModel : class
    {
        if (body == null)
        {
            controller.ModelState.AddModelError("body", "A body is required");
            return null;
        }

        var entity = await mapper
            .FromAsync(body, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
        {
            throw new Exception("Update view model mapper did not map to entity.");
        }

        if (!controller.TryValidateModel(entity))
        {
            return null;
        }

        return entity;
    }

}
