using System;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Implementations;

public class EntityExportMapper<TEntity, TVersion, TOut> : IEntityExportMapper<TEntity, TVersion, TOut>
    where TEntity : class
    where TVersion : class, IAutoCrudApiVersion
    where TOut : class
{
    private readonly Func<TEntity, TOut> _func;

    public EntityExportMapper(Func<TEntity, TOut> func)
    {
        _func = func;
    }

    public TOut Map(TEntity entity) => _func?.Invoke(entity);
}
