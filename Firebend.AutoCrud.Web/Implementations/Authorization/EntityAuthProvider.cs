using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public class EntityAuthProvider
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;

    public EntityAuthProvider(IAuthorizationService authorizationService, IServiceProvider serviceProvider)
    {
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;
    }

    public Task<TEntity> GetEntityAsync<TKey, TEntity>(string entityIdString, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var keyParser = _serviceProvider.GetService<IEntityKeyParser<TKey, TEntity>>();
        if (keyParser == null)
        {
            throw new Exception($"Cannot resolve key parser for {nameof(TEntity)}");
        }

        var entityId = keyParser.ParseKey(entityIdString);
        if (entityId == null)
        {
            throw new Exception($"Failed to parse id for {nameof(TEntity)}");
        }

        return GetEntityAsync<TKey, TEntity>(entityId.Value, cancellationToken);
    }

    public Task<TEntity> GetEntityAsync<TKey, TEntity>(TKey id, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var readService = _serviceProvider.GetService<IEntityReadService<TKey, TEntity>>();
        if (readService == null)
        {
            throw new Exception($"Cannot resolve read service for {nameof(TEntity)}");
        }

        return readService.GetByKeyAsync(id, cancellationToken);
    }

    public async Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(string entityIdString,
        ClaimsPrincipal user, string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var entity = await GetEntityAsync<TKey, TEntity>(entityIdString, cancellationToken);
        return await AuthorizeEntityAsync(user, entity, policy);
    }

    public async Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(TKey id, ClaimsPrincipal user,
        string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var entity = await GetEntityAsync<TKey, TEntity>(id, cancellationToken);
        return await AuthorizeEntityAsync(user, entity, policy);
    }

    public async Task<AuthorizationResult> AuthorizeEntityAsync(ClaimsPrincipal user, object entity, string policy) =>
        await _authorizationService.AuthorizeAsync(user, entity, policy);
}
