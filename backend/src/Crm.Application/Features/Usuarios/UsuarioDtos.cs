using Crm.Domain.Enums;

namespace Crm.Application.Features.Usuarios;

public record UsuarioDto(Guid Id, string Nome, string Email, Papel Papel, Guid ClinicaId);

public record ProfissionalListItem(Guid Id, string Nome);

/// <summary>Item da lista de gestão de equipe (inclui status ativo).</summary>
public record EquipeUsuarioDto(Guid Id, string Nome, string Email, Papel Papel, bool Ativo);

/// <summary>Criação de um membro da equipe pelo Admin (senha inicial definida por ele).</summary>
public record CriarUsuarioRequest(string Nome, string Email, string Senha, Papel Papel);

/// <summary>Edição de um membro: nome, papel, status e, opcionalmente, redefinir a senha.</summary>
public record AtualizarUsuarioRequest(string Nome, Papel Papel, bool Ativo, string? NovaSenha);
