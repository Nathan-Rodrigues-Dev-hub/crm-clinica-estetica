using Crm.Domain.Enums;

namespace Crm.Application.Features.Auth;

public record LoginRequest(string Email, string Senha);

public record UsuarioLogado(Guid Id, string Nome, string Email, Papel Papel, Guid ClinicaId);

public record LoginResponse(string Token, DateTime ExpiraEm, UsuarioLogado Usuario);
