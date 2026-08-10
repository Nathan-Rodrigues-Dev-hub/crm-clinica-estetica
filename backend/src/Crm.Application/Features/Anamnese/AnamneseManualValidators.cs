using FluentValidation;

namespace Crm.Application.Features.Anamnese;

public class CriarAnamneseManualRequestValidator : AbstractValidator<CriarAnamneseManualRequest>
{
    public CriarAnamneseManualRequestValidator()
    {
        RuleFor(x => x.Peso).InclusiveBetween(1m, 500m).When(x => x.Peso is not null)
            .WithMessage("Peso deve estar entre 1 e 500 kg.");
        RuleFor(x => x.Altura).InclusiveBetween(0.5m, 2.5m).When(x => x.Altura is not null)
            .WithMessage("Informe a altura em metros (ex.: 1,70) — entre 0,5 e 2,5 m.");
        RuleFor(x => x.DataPreenchimento)
            .Must(d => d is null || d.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("Data de preenchimento não pode ser no futuro.");
    }
}
