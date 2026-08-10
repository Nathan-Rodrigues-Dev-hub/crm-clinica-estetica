using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Agenda;
using Crm.Application.Features.Anamnese;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Triagem;

public class TriagemService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IClock _clock;
    private readonly AgendaService _agenda;

    public TriagemService(IAppDbContext db, ICurrentUser user, IClock clock, AgendaService agenda)
    {
        _db = db;
        _user = user;
        _clock = clock;
        _agenda = agenda;
    }

    /// <summary>Fila de triagem. Por padrão mostra as pendentes (Recebida/EmTriagem).</summary>
    public async Task<Result<List<SolicitacaoListItem>>> ListarFilaAsync(StatusSolicitacao? status, CancellationToken ct = default)
    {
        var query = _db.Solicitacoes.AsNoTracking().Where(s => s.ClinicaId == _user.ClinicaId);
        query = status is { } st
            ? query.Where(s => s.Status == st)
            : query.Where(s => s.Status == StatusSolicitacao.Recebida || s.Status == StatusSolicitacao.EmTriagem);

        var itens = await query
            .OrderBy(s => s.DataEnvio)
            .Select(s => new SolicitacaoListItem(
                s.Id, s.PacienteId, s.Paciente!.Nome, s.Paciente.Telefone,
                s.ProcedimentoInteresse != null ? s.ProcedimentoInteresse.Nome : (s.ProcedimentoInteresseTexto ?? "—"),
                s.Status, s.DataEnvio))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }

    /// <summary>Detalhe com a anamnese (dado sensível — controlar acesso no controller).</summary>
    public async Task<Result<SolicitacaoDetalhe>> ObterAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _db.Solicitacoes.AsNoTracking()
            .Include(x => x.Paciente)
            .Include(x => x.Anamnese)
            .Include(x => x.ProcedimentoInteresse)
            .FirstOrDefaultAsync(x => x.Id == id && x.ClinicaId == _user.ClinicaId, ct);

        if (s is null) return Result.Falha<SolicitacaoDetalhe>("Solicitação não encontrada.", ErrorType.NotFound);

        var a = s.Anamnese!;
        var dto = new SolicitacaoDetalhe(
            s.Id, s.PacienteId, s.Paciente!.Nome, s.Paciente.Telefone, s.Paciente.Cpf,
            s.ProcedimentoInteresse?.Nome ?? s.ProcedimentoInteresseTexto ?? "—",
            s.Status, s.DataEnvio, s.MotivoRecusa, s.AgendamentoId,
            new AnamneseDetalhe(a.Id, a.DataPreenchimento, a.Peso, a.Altura, a.Imc,
                a.PressaoArterial, a.MotivoEstetico, a.AceiteDeclaracao, a.Respostas));

        return Result.Ok(dto);
    }

    /// <summary>Assume a triagem (marca EmTriagem).</summary>
    public async Task<Result> AssumirAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _db.Solicitacoes.FirstOrDefaultAsync(x => x.Id == id && x.ClinicaId == _user.ClinicaId, ct);
        if (s is null) return Result.Falha("Solicitação não encontrada.", ErrorType.NotFound);
        if (s.Status != StatusSolicitacao.Recebida && s.Status != StatusSolicitacao.EmTriagem)
            return Result.Falha($"Solicitação já está '{s.Status}'.", ErrorType.Conflict);

        s.Status = StatusSolicitacao.EmTriagem;
        s.UsuarioTriagemId = _user.UsuarioId;
        await _db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>Aprova: cria o agendamento vinculado e marca a solicitação como Agendada.</summary>
    public async Task<Result<DecisaoResultado>> AgendarAsync(Guid id, AgendarDecisaoRequest req, CancellationToken ct = default)
    {
        var s = await _db.Solicitacoes.FirstOrDefaultAsync(x => x.Id == id && x.ClinicaId == _user.ClinicaId, ct);
        if (s is null) return Result.Falha<DecisaoResultado>("Solicitação não encontrada.", ErrorType.NotFound);
        if (s.Status is StatusSolicitacao.Agendada or StatusSolicitacao.Recusada)
            return Result.Falha<DecisaoResultado>($"Solicitação já foi decidida ('{s.Status}').", ErrorType.Conflict);

        var procedimentoId = req.ProcedimentoId ?? s.ProcedimentoInteresseId;
        if (procedimentoId is null)
            return Result.Falha<DecisaoResultado>("Informe o procedimento a agendar (a solicitação não tem procedimento do catálogo).");

        var criar = new CriarAgendamentoRequest(s.PacienteId, req.UsuarioProfissionalId, procedimentoId.Value, req.Inicio, req.ValorCobrado, req.Observacao);
        var ag = await _agenda.CriarAsync(criar, s.Id, ct);
        if (!ag.Sucesso) return Result.Falha<DecisaoResultado>(ag.Erro!, ag.TipoErro);

        s.Status = StatusSolicitacao.Agendada;
        s.AgendamentoId = ag.Valor!.Id;
        s.UsuarioTriagemId = _user.UsuarioId;
        s.DataDecisao = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        var msg = $"Olá {s.Paciente?.Nome ?? "paciente"}! Seu atendimento foi agendado para " +
                  $"{ag.Valor.Inicio:dd/MM/yyyy HH:mm} (UTC). Qualquer dúvida, estamos à disposição.";
        return Result.Ok(new DecisaoResultado(s.Id, s.Status, s.AgendamentoId, msg));
    }

    /// <summary>Recusa: registra o motivo e marca a solicitação como Recusada.</summary>
    public async Task<Result<DecisaoResultado>> RecusarAsync(Guid id, RecusarDecisaoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Motivo))
            return Result.Falha<DecisaoResultado>("Informe o motivo da recusa.");

        var s = await _db.Solicitacoes.Include(x => x.Paciente)
            .FirstOrDefaultAsync(x => x.Id == id && x.ClinicaId == _user.ClinicaId, ct);
        if (s is null) return Result.Falha<DecisaoResultado>("Solicitação não encontrada.", ErrorType.NotFound);
        if (s.Status is StatusSolicitacao.Agendada or StatusSolicitacao.Recusada)
            return Result.Falha<DecisaoResultado>($"Solicitação já foi decidida ('{s.Status}').", ErrorType.Conflict);

        s.Status = StatusSolicitacao.Recusada;
        s.MotivoRecusa = req.Motivo.Trim();
        s.UsuarioTriagemId = _user.UsuarioId;
        s.DataDecisao = _clock.UtcNow;
        await _db.SaveChangesAsync(ct);

        var msg = $"Olá {s.Paciente?.Nome ?? "paciente"}. Após avaliarmos suas informações, não será possível " +
                  $"realizar o procedimento neste momento. Motivo: {s.MotivoRecusa}. Estamos à disposição para orientar alternativas.";
        return Result.Ok(new DecisaoResultado(s.Id, s.Status, null, msg));
    }
}
