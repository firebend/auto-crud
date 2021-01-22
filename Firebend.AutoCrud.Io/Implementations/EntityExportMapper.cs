using System;
using Firebend.AutoCrud.Core.Pooling;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityExportMapper<TEntity, TOut> : IEntityExportMapper<TEntity, TOut>
        where TEntity : class
        where TOut : class
    {
        private readonly Func<TEntity, TOut> _func;

        public EntityExportMapper()
        {

        }

        public EntityExportMapper(Func<TEntity, TOut> func)
        {
            _func = func;
        }

        public TOut Map(TEntity entity)
        {
            if (_func == null)
            {
                return null;
            }

            using var _ = AutoCrudDelegatePool.GetPooledFunction(_func, entity, out var pooledFunc);
            var mapped = pooledFunc();
            return mapped;
        }
    }
}
