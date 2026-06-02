using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Sreda.Backend.API.Common;

/// <summary>
/// Глобальный обработчик исключений: преобразует исключения в ProblemDetails.
/// Ошибки валидации (FluentValidation) → 400 с деталями полей, остальное → 500.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails;

        switch (exception)
        {
            case ValidationException validationException:
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Помилка валідації",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                };
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            default:
                _logger.LogError(exception, "Необроблений виняток: {Message}", exception.Message);
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Внутрішня помилка сервера",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
                };
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
