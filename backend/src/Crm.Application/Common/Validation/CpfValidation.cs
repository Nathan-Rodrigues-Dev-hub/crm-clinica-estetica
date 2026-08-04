using FluentValidation;

namespace Crm.Application.Common.Validation;

/// <summary>Regras de validação reutilizáveis para os requests.</summary>
public static class ValidationExtensions
{
    /// <summary>Valida um CPF opcional: se informado, precisa ter DV válido. Vazio/nulo é aceito (CPF pendente).</summary>
    public static IRuleBuilderOptions<T, string?> CpfOpcionalValido<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(cpf => string.IsNullOrWhiteSpace(cpf) || Cpf.Valido(Cpf.Normalizar(cpf)))
            .WithMessage("CPF inválido (verifique os dígitos).");
}
