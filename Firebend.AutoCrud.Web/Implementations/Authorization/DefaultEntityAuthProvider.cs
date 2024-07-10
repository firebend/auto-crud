using System;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public class DefaultEntityAuthProvider : EntityAuthProvider
{
    public DefaultEntityAuthProvider(IAuthorizationService authorizationService,
        IServiceProvider serviceProvider) : base(authorizationService, serviceProvider)
    {
    }
}
