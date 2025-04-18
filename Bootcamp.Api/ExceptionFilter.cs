using Bootcamp.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bootcamp.Api;

public class ExceptionFilter : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case MlServiceException serviceException:
                context.Response.StatusCode = 418;
                await context.Response.WriteAsJsonAsync(new { message = serviceException.Message}, cancellationToken: cancellationToken);
                break;
            
            default:
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { message = exception.Message }, cancellationToken: cancellationToken);
                break;
        }
        
        return true;
    }
}