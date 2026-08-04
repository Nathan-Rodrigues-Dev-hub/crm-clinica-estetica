using Crm.Application.Common.Validation;
using FluentValidation;

namespace Crm.Application.Features.Pacientes;

public class CriarPacienteRequestValidator : AbstractValidator<CriarPacienteRequest>
{
    public CriarPacienteRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Cpf).CpfOpcionalValido();
        RuleFor(x => x.Telefone).MaximumLength(30);
    }
}

public class AtualizarPacienteRequestValidator : AbstractValidator<AtualizarPacienteRequest>
{
    public AtualizarPacienteRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(200);
        RuleFor(x => x.Cpf).CpfOpcionalValido();
        RuleFor(x => x.Telefone).MaximumLength(30);
    }
}
