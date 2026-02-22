using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Models;

namespace Shared.Application.Controller;

public class BaseApiController(ISender mediator) : ControllerBase
{
    protected async Task<IActionResult> SendQuery<T>(IRequest<Result<T>> query)
        => HandleResult(await mediator.Send(query));

    protected async Task<IActionResult> SendCommand<T>(IRequest<Result<T>> command)
        => HandleResult(await mediator.Send(command));

    private IActionResult HandleResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Value) : HandleError(result.Error);

    private IActionResult HandleError(Error error)
    {
        if (error.ValidationErrors?.Any() == true)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = error.Description,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            };

            foreach (var validationError in error.ValidationErrors)
                problemDetails.Errors.Add(validationError.Key, validationError.Value);

            return ValidationProblem(problemDetails);
        }

        return error.Code switch
        {
            var c when c.Contains("NotFound") => NotFound(new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = error.Description,
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            }),
            var c when c.Contains("Unauthorized") => StatusCode(StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = error.Description,
                Status = StatusCodes.Status401Unauthorized,
                Instance = HttpContext.Request.Path
            }),
            var c when c.Contains("Conflict") => Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = error.Description,
                Status = StatusCodes.Status409Conflict,
                Instance = HttpContext.Request.Path
            }),
            _ => BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = error.Description,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            })
        };
    }
}