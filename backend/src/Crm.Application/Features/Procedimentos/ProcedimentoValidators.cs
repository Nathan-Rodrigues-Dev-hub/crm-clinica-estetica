using FluentValidation;

namespace Crm.Application.Features.Procedimentos;

public class CriarProcedimentoRequestValidator : AbstractValidator<CriarProcedimentoRequest>
{
    public CriarProcedimentoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.DuracaoMinutos).GreaterThan(0).WithMessage("Duração deve ser maior que zero.");
        RuleFor(x => x.ValorReferencia).GreaterThanOrEqualTo(0).WithMessage("Valor de referência não pode ser negativo.");
    }
}

public class AtualizarProcedimentoRequestValidator : AbstractValidator<AtualizarProcedimentoRequest>
{
    public AtualizarProcedimentoRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.DuracaoMinutos).GreaterThan(0).WithMessage("Duração deve ser maior que zero.");
        RuleFor(x => x.ValorReferencia).GreaterThanOrEqualTo(0).WithMessage("Valor de referência não pode ser negativo.");
    }
}
