using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations
{
    public class FunctionViewModelMapper<TKey, TEntity, TViewModel> : IViewModelMapper<TKey, TEntity, TViewModel>
        where TViewModel : class
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        public Func<TEntity, TViewModel> To { get; set; }

        public Func<TViewModel, TEntity> From { get; set; }

        public Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken = default)
            => Task.FromResult(From(model));

        public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken = default)
            => Task.FromResult(model.Select(From));

        public Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken = default)
            => Task.FromResult(To(entity));

        public Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity.Select(To));
    }
}
