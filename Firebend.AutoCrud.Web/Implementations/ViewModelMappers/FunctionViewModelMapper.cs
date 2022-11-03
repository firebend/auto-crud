using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class FunctionViewModelMapper<TKey, TEntity, TViewModel, TViewModelBody> : ICreateViewModelMapper<TKey, TEntity, TViewModel>,
        IUpdateViewModelMapper<TKey, TEntity, TViewModel>,
        IReadViewModelMapper<TKey, TEntity, TViewModel>
        where TViewModel : class
        where TViewModelBody : class
        where TEntity : class, IEntity<TKey>
        where TKey : struct
    {
        private static Func<TEntity, TViewModel> _to;
        private static Func<TViewModel, TEntity> _from;

        public FunctionViewModelMapper()
        {

        }

        public FunctionViewModelMapper(Func<TEntity, TViewModel> to)
        {
            _to = to;
        }

        public FunctionViewModelMapper(Func<TViewModel, TEntity> from)
        {
            _from = from;
        }

        public FunctionViewModelMapper(Func<TViewModel, TEntity> from, Func<TEntity, TViewModel> to)
        {
            _from = from;
            _to = to;
        }

        private static TEntity ToEntity(TViewModel model)
            => _from?.Invoke(model);

        private static TViewModel ToViewModel(TEntity entity)
            => _to?.Invoke(entity);

        public Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken = default)
            => Task.FromResult(ToEntity(model));

        public Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken = default)
            => _from == null ? Task.FromResult((IEnumerable<TEntity>)null) : Task.FromResult(model.Select(ToEntity));

        public Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken = default)
            => Task.FromResult(ToViewModel(entity));

        public Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default)
            => _to == null ? Task.FromResult((IEnumerable<TViewModel>)null) : Task.FromResult(entity.Select(ToViewModel));
    }
}
