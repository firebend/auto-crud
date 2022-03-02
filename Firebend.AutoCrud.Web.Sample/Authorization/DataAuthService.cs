using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Firebend.AutoCrud.Web.Sample.Authorization;

public class DataAuthService
{
    public Task<bool> AuthorizeAsync(ClaimsPrincipal user, IDataAuth dataAuth)
    {
        if (dataAuth == null)
        {
            return Task.FromResult(true);
        }

        if (dataAuth.UserEmails.IsEmpty())
        {
            return Task.FromResult(true);
        }

        var email = user.Claims.FirstOrDefault(claim => claim.Type.EndsWith("emailaddress"))?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }
        var hasUserEmail = dataAuth.UserEmails.Any(userEmail => userEmail.Equals(email));
        return Task.FromResult(hasUserEmail);
    }
}
