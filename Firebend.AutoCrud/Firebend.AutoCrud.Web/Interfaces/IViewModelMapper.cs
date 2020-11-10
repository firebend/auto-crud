namespace Firebend.AutoCrud.Web.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interfaces.Models;

    public interface IViewModelMapper<TKey, TEntity, TViewModel>
        where TEntity : IEntity<TKey>
        where TKey : struct
        where TViewModel : class
    {
        Task<TEntity> FromAsync(TViewModel model, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FromAsync(IEnumerable<TViewModel> model, CancellationToken cancellationToken = default);

        Task<TViewModel> ToAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<IEnumerable<TViewModel>> ToAsync(IEnumerable<TEntity> entity, CancellationToken cancellationToken = default);
    }
}
