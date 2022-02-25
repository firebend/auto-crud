using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class ReadAllAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceReadAll";
}
