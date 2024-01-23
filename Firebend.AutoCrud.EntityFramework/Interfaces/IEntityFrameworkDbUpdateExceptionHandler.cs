using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkDbUpdateExceptionHandler<TKey, in TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    bool HandleException(IDbContext context, TEntity entity, DbUpdateException exception);
}
