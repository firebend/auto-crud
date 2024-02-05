using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CustomSearchParameters : ModifiedEntitySearchRequest
{
    public string NickName { get; set; }
}
