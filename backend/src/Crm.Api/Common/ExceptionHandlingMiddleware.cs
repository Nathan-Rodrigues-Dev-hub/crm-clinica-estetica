using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Common;

/// <summary>
/// Catch-all para exceções não tratadas. Erros de negócio já viram Result/ProblemDetails
/// nos controllers; aqui tratamos o inesperado, logando de forma estruturada e devolvendo
/// um ProblemDetails 500 sem vazar detalhes internos.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogError(ex,
                "Erro não tratado ao processar {Method} {Path} (traceId: {TraceId})",
                context.Request.Method, context.Request.Path, traceId);

            if (context.Response.HasStarted)
            {
                // Não é possível reescrever a resposta; apenas propaga.
                throw;
            }

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno do servidor.",
                Detail = _env.IsDevelopment() ? ex.ToString() : "Ocorreu um erro inesperado. Tente novamente.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
            problem.Extensions["traceId"] = traceId;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
