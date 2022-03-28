using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public interface IEntityAuthProvider
{
    Task<TEntity> GetEntityAsync<TKey, TEntity>(string entityIdString, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>;

    Task<TEntity> GetEntityAsync<TKey, TEntity>(TKey id, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>;

    Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(string entityIdString,
        ClaimsPrincipal user, string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>;

    Task<AuthorizationResult> AuthorizeEntityAsync<TKey, TEntity>(TKey id, ClaimsPrincipal user,
        string policy, CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>;

    Task<AuthorizationResult> AuthorizeEntityReadAsync<TKey, TEntity>(TKey id, ClaimsPrincipal user,
        CancellationToken cancellationToken)
        where TKey : struct
        where TEntity : class, IEntity<TKey>;

    Task<AuthorizationResult> AuthorizeEntityAsync(ClaimsPrincipal user, object entity, string policy);
}
