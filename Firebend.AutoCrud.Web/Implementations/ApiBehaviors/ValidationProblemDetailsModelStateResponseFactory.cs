using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Implementations.ApiBehaviors;

public static class ValidationProblemDetailsModelStateResponseFactory
{
    private const string Type = "https://httpstatuses.com/400";
    private const string Detail = "One or more validation errors occurred.";
    private const int Status = StatusCodes.Status400BadRequest;

    public static IActionResult InvalidModelStateResponseFactory(ActionContext context)
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Instance = context.HttpContext.Request.Path,
            Status = Status,
            Type = Type,
            Detail = Detail
        };

        return new BadRequestObjectResult(problemDetails);
    }
}
