using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Rocland.Api.Middleware;

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
        // Registramos el error en Serilog con todo el StackTrace internamente
        _logger.LogError(exception, "Excepción no controlada capturada por el middleware global.");

        // Configuramos la respuesta HTTP
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Devolvemos el formato estandarizado ProblemDetails (RFC 7807)
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Error interno del servidor",
            Detail = "Ha ocurrido un error inesperado al procesar la solicitud. Contacte a soporte técnico.",
            Instance = httpContext.Request.Path
        }, cancellationToken);

        // Retornamos true para indicar que la excepción ya fue manejada y no debe propagarse más
        return true;
    }
}
