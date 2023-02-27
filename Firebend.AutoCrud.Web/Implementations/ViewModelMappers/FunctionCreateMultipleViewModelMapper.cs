using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class FunctionCreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView> : ICreateMultipleViewModelMapper<TKey, TEntity, TVersion, TViewWrapper, TView>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TVersion : class, IApiVersion
        where TViewWrapper : IMultipleEntityViewModel<TView>
    {
        private static Func<TViewWrapper, TView, TEntity> _func;

        public FunctionCreateMultipleViewModelMapper(Func<TViewWrapper, TView, TEntity> func)
        {
            _func = func;
        }

        public Task<TEntity> FromAsync(TViewWrapper wrapper, TView viewModel, CancellationToken cancellationToken = default)
            => Task.FromResult(_func(wrapper, viewModel));
    }
}
