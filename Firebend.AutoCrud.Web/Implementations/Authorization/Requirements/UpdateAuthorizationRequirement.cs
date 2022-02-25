using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class UpdateAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceUpdate";
}
