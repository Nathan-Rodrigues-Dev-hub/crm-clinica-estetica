using FluentValidation;

namespace Crm.Application.Features.Agenda;

public class CriarAgendamentoRequestValidator : AbstractValidator<CriarAgendamentoRequest>
{
    public CriarAgendamentoRequestValidator()
    {
        RuleFor(x => x.PacienteId).NotEmpty().WithMessage("Paciente é obrigatório.");
        RuleFor(x => x.UsuarioProfissionalId).NotEmpty().WithMessage("Profissional é obrigatório.");
        RuleFor(x => x.ProcedimentoId).NotEmpty().WithMessage("Procedimento é obrigatório.");
        RuleFor(x => x.Inicio).NotEmpty().WithMessage("Data/hora de início é obrigatória.");
        RuleFor(x => x.ValorCobrado).GreaterThanOrEqualTo(0).When(x => x.ValorCobrado.HasValue)
            .WithMessage("Valor cobrado não pode ser negativo.");
    }
}
