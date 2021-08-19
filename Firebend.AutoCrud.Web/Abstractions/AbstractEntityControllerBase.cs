using Firebend.AutoCrud.Web.Implementations.ActionResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public abstract class AbstractEntityControllerBase : ControllerBase
    {
        private readonly IOptions<ApiBehaviorOptions> _apiOptions;

        protected AbstractEntityControllerBase(IOptions<ApiBehaviorOptions> apiOptions)
        {
            _apiOptions = apiOptions;
        }

        protected virtual WrappedActionResult GetInvalidModelStateResult() => _apiOptions.Value.WrapInvalidModelStateResult(ControllerContext);
    }
}
