using Crm.Application.Common.Validation;
using FluentValidation;

namespace Crm.Application.Features.Anamnese;

public class IntakeRequestValidator : AbstractValidator<IntakeRequest>
{
    public IntakeRequestValidator()
    {
        RuleFor(x => x.Paciente).NotNull().WithMessage("Dados do paciente são obrigatórios.");
        RuleFor(x => x.Paciente.Nome).NotEmpty().WithMessage("Nome é obrigatório.")
            .When(x => x.Paciente is not null);
        RuleFor(x => x.Paciente.Cpf).CpfOpcionalValido().When(x => x.Paciente is not null);
        RuleFor(x => x.Anamnese).NotNull().WithMessage("Dados da anamnese são obrigatórios.");
        RuleFor(x => x.Anamnese.AceiteDeclaracao).Equal(true)
            .WithMessage("É necessário aceitar a declaração de veracidade.")
            .When(x => x.Anamnese is not null);
        RuleFor(x => x.Anamnese.Peso).GreaterThan(0).When(x => x.Anamnese?.Peso is not null)
            .WithMessage("Peso deve ser maior que zero.");
        RuleFor(x => x.Anamnese.Altura).GreaterThan(0).When(x => x.Anamnese?.Altura is not null)
            .WithMessage("Altura deve ser maior que zero.");
    }
}
