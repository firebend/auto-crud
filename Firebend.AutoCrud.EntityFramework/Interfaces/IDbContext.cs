using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;


public interface IDbContext : IDisposable, IAsyncDisposable
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    public DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    public DatabaseFacade Database { get; }

    public IModel Model { get; }

    public DbContextOptions Options { get; }

    public bool UseUserDefinedTransaction { get; set; }
}
