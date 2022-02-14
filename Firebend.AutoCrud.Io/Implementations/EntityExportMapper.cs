using System;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityExportMapper<TEntity, TOut> : IEntityExportMapper<TEntity, TOut>
        where TEntity : class
        where TOut : class
    {
        private readonly Func<TEntity, TOut> _func;

        public EntityExportMapper(Func<TEntity, TOut> func)
        {
            _func = func;
        }

        public TOut Map(TEntity entity) => _func?.Invoke(entity);
    }
}
