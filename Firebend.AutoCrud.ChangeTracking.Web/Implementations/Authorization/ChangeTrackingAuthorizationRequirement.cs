using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;

public class ChangeTrackingAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceChangeTracking";
}
