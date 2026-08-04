using Crm.Domain.Enums;

namespace Crm.Application.Features.Usuarios;

public record UsuarioDto(Guid Id, string Nome, string Email, Papel Papel, Guid ClinicaId);

public record ProfissionalListItem(Guid Id, string Nome);
