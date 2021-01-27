using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class FunctionCreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView> : ICreateMultipleViewModelMapper<TKey, TEntity, TViewWrapper, TView>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TViewWrapper : IMultipleEntityViewModel<TView>
    {
        public Func<TViewWrapper, TView, TEntity> Func { get; set; }

        public Task<TEntity> FromAsync(TViewWrapper wrapper, TView viewModel, CancellationToken cancellationToken = default)
        {
            using var _ = AutoCrudDelegatePool.GetPooledFunction(Func, viewModel, out var func);
            var entity = func(wrapper);
            return Task.FromResult(entity);
        }
    }
}
