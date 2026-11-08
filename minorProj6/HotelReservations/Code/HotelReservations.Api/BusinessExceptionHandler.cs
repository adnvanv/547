using HotelReservations.Business;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservations.Api;

/// <summary>
/// Turns business-layer exceptions into JSON problem details so controllers don't need try/catch:
/// NotFoundException -> 404, BusinessRuleException -> 400, ExternalServiceException (an Azure function
/// call failed) -> 502. Anything else falls through to a 500.
/// </summary>
public class BusinessExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Request rejected"),
            ExternalServiceException => (StatusCodes.Status502BadGateway, "Azure function unavailable"),
            _ => (0, ""),
        };
        if (status == 0)
            return false;

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
