using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class SearchAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceSearch";
}
