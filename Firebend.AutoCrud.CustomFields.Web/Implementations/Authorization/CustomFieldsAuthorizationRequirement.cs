using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.CustomFields.Web.Implementations.Authorization;

public class CustomFieldsAuthorizationRequirement : IAuthorizationRequirement
{
    public const string DefaultPolicy = "ResourceCustomFields";
}
