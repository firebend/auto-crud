using Firebend.AutoCrud.Web.Implementations.ActionResults;
using Firebend.AutoCrud.Web.Implementations.ApiBehaviors;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web
{
    public static class ApiBehaviorOptionsExtensions
    {
        public static WrappedActionResult WrapInvalidModelStateResult(this ApiBehaviorOptions source, ActionContext context)
        {
            var factory = source.InvalidModelStateResponseFactory ??
                          ValidationProblemDetailsModelStateResponseFactory.InvalidModelStateResponseFactory;

            var response = factory(context);

            return new WrappedActionResult(response);
        }
    }
}
