using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IMaxExportPageSize<TKey, TEntity, TVersion> : IMaxPageSize<TKey, TEntity, TVersion>
    where TEntity : IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion;
