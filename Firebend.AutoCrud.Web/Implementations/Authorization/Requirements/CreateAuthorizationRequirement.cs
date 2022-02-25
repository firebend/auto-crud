using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class CreateAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceCreate";
}
