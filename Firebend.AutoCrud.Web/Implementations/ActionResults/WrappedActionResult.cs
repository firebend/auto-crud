using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Implementations.ActionResults
{
    public class WrappedActionResult : ActionResult
    {
        private readonly IActionResult _actionResult;

        public WrappedActionResult(IActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public override void ExecuteResult(ActionContext context) => ExecuteResultAsync(context).GetAwaiter().GetResult();

        public override Task ExecuteResultAsync(ActionContext context) => _actionResult.ExecuteResultAsync(context);

    }
}
