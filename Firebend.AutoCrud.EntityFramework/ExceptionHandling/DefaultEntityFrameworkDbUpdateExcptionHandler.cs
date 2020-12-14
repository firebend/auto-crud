using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.ExceptionHandling
{
    public class DefaultEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> : IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public bool HandleException(IDbContext context, TEntity entity, DbUpdateException exception) => false;
    }
}
