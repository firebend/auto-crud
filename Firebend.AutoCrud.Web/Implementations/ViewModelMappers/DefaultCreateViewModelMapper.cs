using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class DefaultCreateViewModelMapper<TKey, TEntity, TVersion> :
        AbstractDefaultCreateUpdateViewModelMapper<TKey, TEntity>,
        ICreateViewModelMapper<TKey, TEntity, TVersion, DefaultCreateUpdateViewModel<TKey, TEntity>>
        where TVersion : class, IAutoCrudApiVersion
        where TEntity : class, IEntity<TKey>
        where TKey : struct;
}
