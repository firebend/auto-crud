using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class DeleteAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceDelete";
}
