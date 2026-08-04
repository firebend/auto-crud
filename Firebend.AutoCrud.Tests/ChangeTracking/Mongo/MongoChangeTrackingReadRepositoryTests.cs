using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.ChangeTracking.Mongo.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Mongo.Interfaces;
using FluentAssertions;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.ChangeTracking.Mongo;

public class ReadRepositoryTestEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class ReadRepositoryCustomRow : ChangeTrackingEntity<Guid, ReadRepositoryTestEntity>
{
    public string RealActorEmail { get; set; }
}

public class InMemoryMongoReadClient<TEntity> : IMongoReadClient<Guid, TEntity>
    where TEntity : IEntity<Guid>
{
    private readonly List<TEntity> _items;

    public InMemoryMongoReadClient(List<TEntity> items)
    {
        _items = items;
    }

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => Task.FromResult(_items.AsQueryable().Where(filter).FirstOrDefault());

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetFirstOrDefaultAsync(filter, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(CancellationToken cancellationToken)
        => Task.FromResult(_items.AsQueryable());

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        CancellationToken cancellationToken)
        => Task.FromResult(firstStageFilters == null ? _items.AsQueryable() : firstStageFilters(_items.AsQueryable()));

    public async Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        CancellationToken cancellationToken)
        => firstStageFilters == null ? _items.AsQueryable() : await firstStageFilters(_items.AsQueryable());

    public Task<IQueryable<TEntity>> GetQueryableAsync(IEntityTransaction entityTransaction, CancellationToken cancellationToken)
        => GetQueryableAsync(cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> firstStageFilters,
        IEntityTransaction entityTransaction, CancellationToken cancellationToken)
        => GetQueryableAsync(firstStageFilters, cancellationToken);

    public Task<IQueryable<TEntity>> GetQueryableAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> firstStageFilters,
        IEntityTransaction entityTransaction, CancellationToken cancellationToken)
        => GetQueryableAsync(firstStageFilters, cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => Task.FromResult(_items.AsQueryable().Where(filter).ToList());

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => GetAllAsync(filter, cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => Task.FromResult(_items.AsQueryable().Any(filter));

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => ExistsAsync(filter, cancellationToken);

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
        => Task.FromResult((long)_items.AsQueryable().Count(filter));

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken)
        => CountAsync(filter, cancellationToken);

    public Task<EntityPagedResponse<TEntity>> GetPagedResponseAsync<TSearchRequest>(IQueryable<TEntity> queryable,
        TSearchRequest searchRequest, CancellationToken cancellationToken)
        where TSearchRequest : IEntitySearchRequest
    {
        var list = queryable.ToList();

        return Task.FromResult(new EntityPagedResponse<TEntity>
        {
            Data = list,
            TotalRecords = list.Count,
            CurrentPage = searchRequest?.PageNumber,
            CurrentPageSize = list.Count
        });
    }
}

[TestFixture]
public class MongoChangeTrackingReadRepositoryTests
{
    [Test]
    public async Task GetChangesByEntityId_WithCustomRowType_ReturnsExtraPropertiesFromStore()
    {
        var entityId = Guid.NewGuid();

        var rows = new List<ReadRepositoryCustomRow>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EntityId = entityId,
                Action = "Added",
                RealActorEmail = "actor@test.com",
                ModifiedDate = DateTimeOffset.UtcNow
            }
        };

        var queryClient = new InMemoryMongoReadClient<ReadRepositoryCustomRow>(rows);
        var sut = new MongoChangeTrackingReadRepository<Guid, ReadRepositoryTestEntity, ReadRepositoryCustomRow>(queryClient, null);

        var result = await sut.GetChangesByEntityId(
            new ChangeTrackingSearchRequest<Guid> { EntityId = entityId },
            CancellationToken.None);

        result.Data.Should().ContainSingle();
        result.Data.Single().RealActorEmail.Should().Be("actor@test.com");
    }
}
