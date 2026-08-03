namespace Crm.Application.Features.Procedimentos;

public record CriarProcedimentoRequest(
    string Nome, string? Descricao, string? Categoria, int DuracaoMinutos, decimal ValorReferencia);

public record AtualizarProcedimentoRequest(
    string Nome, string? Descricao, string? Categoria, int DuracaoMinutos, decimal ValorReferencia, bool Ativo);

public record ProcedimentoDto(
    Guid Id, string Nome, string? Descricao, string? Categoria, int DuracaoMinutos, decimal ValorReferencia, bool Ativo);
