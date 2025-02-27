using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public interface IEntityAuthProvider
{
    public Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity, TVersion>(string entityIdString,
        ClaimsPrincipal user, string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion;

    public Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity, TVersion>(TKey id, ClaimsPrincipal user,
        string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion;

    public Task<AuthorizationResult> AuthorizeEntityReadAsync<TKey, TEntity, TVersion>(TKey id, ClaimsPrincipal user,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion;

    public Task<AuthorizationResult> AuthorizeEntityAsync(ClaimsPrincipal user, object entity, string policy);
}
