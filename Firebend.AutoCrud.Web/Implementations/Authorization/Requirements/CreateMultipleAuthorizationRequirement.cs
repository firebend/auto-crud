using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class CreateMultipleAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceCreateMultiple";
}
