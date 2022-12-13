using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Exceptions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public abstract class EntityAuthProvider : IEntityAuthProvider
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;

    protected EntityAuthProvider(IAuthorizationService authorizationService, IServiceProvider serviceProvider)
    {
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;
    }

    private TKey GetEntityKeyAsync<TKey, TEntity>(string entityIdString)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var keyParser = _serviceProvider.GetService<IEntityKeyParser<TKey, TEntity>>();
        if (keyParser == null)
        {
            throw new DependencyResolverException($"Cannot resolve key parser for {nameof(TEntity)}");
        }

        var entityId = keyParser.ParseKey(entityIdString);
        if (entityId == null)
        {
            throw new ArgumentException($"Failed to parse id for {nameof(TEntity)}");
        }

        return entityId.Value;
    }

    protected virtual Task<TEntity> GetEntityAsync<TKey, TEntity>(TKey id, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var readService = _serviceProvider.GetService<IEntityReadService<TKey, TEntity>>();

        if (readService == null)
        {
            throw new DependencyResolverException($"Cannot resolve read service for {nameof(TEntity)}");
        }

        using (readService)
        {
            return readService.GetByKeyAsync(id, cancellationToken);
        }
    }

    public virtual async Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(string entityIdString,
        ClaimsPrincipal user, string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var entityId = GetEntityKeyAsync<TKey, TEntity>(entityIdString);
        return await AuthorizeEntityAsync<TKey, TEntity>(entityId, user, policy, cancellationToken);
    }

    public virtual async Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(TKey id, ClaimsPrincipal user,
        string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var entity = await GetEntityAsync<TKey, TEntity>(id, cancellationToken);
        return await AuthorizeEntityAsync(user, entity, policy);
    }

    public virtual Task<AuthorizationResult> AuthorizeEntityReadAsync<TKey, TEntity>(TKey id, ClaimsPrincipal user,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey> =>
        AuthorizeEntityAsync<TKey, TEntity>(id, user, ReadAuthorizationRequirement.DefaultPolicy, cancellationToken);

    public virtual async Task<AuthorizationResult> AuthorizeEntityAsync(ClaimsPrincipal user, object entity, string policy) =>
        await _authorizationService.AuthorizeAsync(user, entity, policy);
}
