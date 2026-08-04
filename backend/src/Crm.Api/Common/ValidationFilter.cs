using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crm.Api.Common;

/// <summary>
/// Valida os argumentos de ação com o <see cref="IValidator{T}"/> correspondente (FluentValidation),
/// se houver um registrado. Em caso de falha, retorna 400 com ProblemDetails padronizado.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _sp;

    public ValidationFilter(IServiceProvider sp) => _sp = sp;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (_sp.GetService(validatorType) is not IValidator validator) continue;

            var ctx = new ValidationContext<object>(arg);
            var result = await validator.ValidateAsync(ctx, context.HttpContext.RequestAborted);
            if (result.IsValid) continue;

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Title = "Um ou mais erros de validação ocorreram.",
                Status = StatusCodes.Status400BadRequest
            });
            return;
        }

        await next();
    }
}
