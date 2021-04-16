using Firebend.AutoCrud.Web.Implementations.ActionResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Abstractions
{
    public class AbstractEntityControllerBase : ControllerBase
    {
        private IOptions<ApiBehaviorOptions> _apiOptions;

        public AbstractEntityControllerBase(IOptions<ApiBehaviorOptions> apiOptions)
        {
            _apiOptions = apiOptions;
        }

        public WrappedActionResult GetInvalidModelStateResult() => _apiOptions.Value.WrapInvalidModelStateResult(ControllerContext);
    }
}
