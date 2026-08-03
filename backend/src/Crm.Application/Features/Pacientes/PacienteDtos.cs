using Crm.Domain.Enums;

namespace Crm.Application.Features.Pacientes;

public record CriarPacienteRequest(
    string Nome, string? Cpf, string? Telefone, DateOnly? DataNascimento,
    string? Sexo, string? Endereco, string? RedeSocial);

public record AtualizarPacienteRequest(
    string Nome, string? Cpf, string? Telefone, DateOnly? DataNascimento,
    string? Sexo, string? Endereco, string? RedeSocial);

public record PacienteDto(
    Guid Id, string Nome, string? Cpf, bool CpfPendente, string? Telefone,
    DateOnly? DataNascimento, string? Sexo, string? Endereco, string? RedeSocial,
    OrigemPaciente Origem, DateTime CriadoEm);

public record PacienteListItem(Guid Id, string Nome, string? Cpf, string? Telefone, bool CpfPendente);

public record DuplicidadeResponse(List<PacienteListItem> PorCpf, List<PacienteListItem> PorTelefone)
{
    public bool HaPossivelDuplicidade => PorCpf.Count > 0 || PorTelefone.Count > 0;
}
