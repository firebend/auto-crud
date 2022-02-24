using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;

public class CreateAuthorizationRequirement : IAuthorizationRequirement { }

// move handlers to the sample project
// keep requirements here
// change extensions as configurations on the sample project
// create an mvcbuilder extension for registering the action filters
