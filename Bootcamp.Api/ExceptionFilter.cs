using Microsoft.AspNetCore.Mvc.Filters;

namespace Bootcamp.Api;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.ExceptionHandled = true;

        switch (context.Exception)
        {
            case MlServiceException serviceException:
            {
                contetext
            }
        }
    }
}