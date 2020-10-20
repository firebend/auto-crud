using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Implementations.Options
{
    public class PostConfigureMvcOptions : IPostConfigureOptions<MvcOptions>
    {
        public void PostConfigure(string name, MvcOptions options)
        {
            
        }
    }
}