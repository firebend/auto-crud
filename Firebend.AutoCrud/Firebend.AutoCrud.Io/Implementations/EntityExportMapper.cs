using System;
using Firebend.AutoCrud.Io.Interfaces;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class EntityExportMapper<TEntity, TOut> : IEntityExportMapper<TEntity, TOut>
        where TEntity : class
        where TOut : class
    {
        public Func<TEntity, TOut> MapperFunc { get; set; }

        public TOut Map(TEntity entity) => MapperFunc(entity);
    }
}