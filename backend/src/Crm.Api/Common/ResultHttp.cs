using Crm.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Common;

/// <summary>Traduz Result/Result&lt;T&gt; em respostas HTTP. Usável por qualquer controller.</summary>
public static class ResultHttp
{
    public static IActionResult ToHttp<T>(this ControllerBase c, Result<T> result, int successStatus = StatusCodes.Status200OK)
        => result.Sucesso ? c.StatusCode(successStatus, result.Valor) : Problema(c, result.TipoErro, result.Erro);

    public static IActionResult ToHttp(this ControllerBase c, Result result, int successStatus = StatusCodes.Status200OK)
        => result.Sucesso ? c.StatusCode(successStatus, new { }) : Problema(c, result.TipoErro, result.Erro);

    private static IActionResult Problema(ControllerBase c, ErrorType tipo, string? erro)
    {
        var status = tipo switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest
        };
        return c.Problem(detail: erro, statusCode: status);
    }
}
