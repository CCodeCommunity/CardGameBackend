using System;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status201Created), ProducesResponseType(StatusCodes.Status400BadRequest)]
public class ErrorController : Controller
{
    [Route("/error-local-development")]
    public IActionResult ErrorLocalDevelopment([FromServices] IWebHostEnvironment webHostEnvironment)
    {
        if (webHostEnvironment.EnvironmentName != "Development")
        {
            throw new InvalidOperationException("This shouldn't be invoked in non-development environments.");
        }

        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context!.Error;

        return Problem(
            detail: context!.Error.StackTrace, 
            title: context.Error.Message,
            statusCode: (int) StatusCodeForException(exception));
    }

    [Route("/error")]
    public IActionResult Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context!.Error;
        
        // if      (exception is MyNotFoundException) code = 404; // Not Found
        // else if (exception is MyAuthException)   code = 401; // Unauthorized
        
        return Problem(statusCode: (int) StatusCodeForException(exception));
    }

    private static HttpStatusCode StatusCodeForException(Exception e)
    {
        return HttpStatusCode.BadRequest;
    }
}