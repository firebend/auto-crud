using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Models;

namespace Firebend.AutoCrud.Web.Implementations.ViewModelMappers
{
    public class DefaultUpdateViewModelMapper<TKey, TEntity, TVersion> :
        AbstractDefaultCreateUpdateViewModelMapper<TKey, TEntity>,
        IUpdateViewModelMapper<TKey, TEntity, TVersion, DefaultCreateUpdateViewModel<TKey, TEntity>>
        where TEntity : class, IEntity<TKey>
        where TKey : struct
        where TVersion : class, IAutoCrudApiVersion;
}
