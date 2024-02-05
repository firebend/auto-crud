using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public class DefaultEntityAuthProvider : EntityAuthProvider
{
    public DefaultEntityAuthProvider(IAuthorizationService authorizationService,
        IServiceScopeFactory serviceScopeFactory) : base(authorizationService, serviceScopeFactory)
    {
    }
}
