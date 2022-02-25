using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class ReadAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceRead";
}
