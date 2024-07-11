using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions;

public class AbstractDbContextSaveRepo<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, new()
{
    protected  IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> ExceptionHandler { get; }

    public AbstractDbContextSaveRepo(IDbContextProvider<TKey, TEntity> provider,
        IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler) : base(provider)
    {
        ExceptionHandler = exceptionHandler;
    }

    protected virtual async Task SaveAsync(TEntity entity, IDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (!(ExceptionHandler?.HandleException(context, entity, ex) ?? false))
            {
                throw;
            }
        }
    }
}
