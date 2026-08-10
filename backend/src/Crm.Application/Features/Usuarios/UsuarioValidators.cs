using FluentValidation;

namespace Crm.Application.Features.Usuarios;

public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-mail é obrigatório.").EmailAddress().WithMessage("E-mail inválido.").MaximumLength(200);
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha inicial é obrigatória.").MinimumLength(6).WithMessage("A senha deve ter ao menos 6 caracteres.");
        RuleFor(x => x.Papel).IsInEnum().WithMessage("Papel inválido.");
    }
}

public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequest>
{
    public AtualizarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(150);
        RuleFor(x => x.Papel).IsInEnum().WithMessage("Papel inválido.");
        RuleFor(x => x.NovaSenha).MinimumLength(6).When(x => !string.IsNullOrEmpty(x.NovaSenha))
            .WithMessage("A nova senha deve ter ao menos 6 caracteres.");
    }
}

public class AlterarSenhaRequestValidator : AbstractValidator<AlterarSenhaRequest>
{
    public AlterarSenhaRequestValidator()
    {
        RuleFor(x => x.SenhaAtual).NotEmpty().WithMessage("Informe a senha atual.");
        RuleFor(x => x.NovaSenha).NotEmpty().WithMessage("Informe a nova senha.")
            .MinimumLength(6).WithMessage("A nova senha deve ter ao menos 6 caracteres.");
    }
}
