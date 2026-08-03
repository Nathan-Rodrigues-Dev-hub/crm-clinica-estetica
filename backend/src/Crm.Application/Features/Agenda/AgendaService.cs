using Crm.Application.Common;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Agenda;

public class AgendaService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _user;

    public AgendaService(IAppDbContext db, ICurrentUser user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Result<AgendamentoDto>> CriarAsync(CriarAgendamentoRequest req, Guid? solicitacaoId = null, CancellationToken ct = default)
    {
        var clinicaId = _user.ClinicaId!.Value;

        var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == req.PacienteId && p.ClinicaId == clinicaId, ct);
        if (paciente is null) return Result.Falha<AgendamentoDto>("Paciente não encontrado.", ErrorType.NotFound);

        var profissional = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == req.UsuarioProfissionalId && u.ClinicaId == clinicaId, ct);
        if (profissional is null) return Result.Falha<AgendamentoDto>("Profissional não encontrado.", ErrorType.NotFound);

        var procedimento = await _db.Procedimentos.FirstOrDefaultAsync(p => p.Id == req.ProcedimentoId && p.ClinicaId == clinicaId, ct);
        if (procedimento is null) return Result.Falha<AgendamentoDto>("Procedimento não encontrado.", ErrorType.NotFound);

        var inicio = ParaUtc(req.Inicio);
        if (inicio <= DateTime.UtcNow) return Result.Falha<AgendamentoDto>("O início deve ser no futuro.");
        var fim = inicio.AddMinutes(procedimento.DuracaoMinutos);

        // RN03 — sem colisão de horário para o mesmo profissional (considera apenas agendamentos ativos).
        var colide = await _db.Agendamentos.AnyAsync(a =>
            a.UsuarioProfissionalId == req.UsuarioProfissionalId &&
            a.Status == StatusAgendamento.Agendado &&
            a.Inicio < fim && a.Fim > inicio, ct);
        if (colide) return Result.Falha<AgendamentoDto>("Já existe um agendamento nesse horário para o profissional.", ErrorType.Conflict);

        var ag = new Agendamento
        {
            ClinicaId = clinicaId,
            PacienteId = req.PacienteId,
            UsuarioProfissionalId = req.UsuarioProfissionalId,
            ProcedimentoId = req.ProcedimentoId,
            SolicitacaoAtendimentoId = solicitacaoId,
            Inicio = inicio,
            Fim = fim,
            Status = StatusAgendamento.Agendado,
            ValorCobrado = req.ValorCobrado ?? procedimento.ValorReferencia,
            Observacao = req.Observacao
        };
        _db.Agendamentos.Add(ag);
        await _db.SaveChangesAsync(ct);

        return Result.Ok(new AgendamentoDto(
            ag.Id, paciente.Id, paciente.Nome, profissional.Id, profissional.Nome,
            procedimento.Id, procedimento.Nome, ag.Inicio, ag.Fim, ag.Status,
            ag.ValorCobrado, ag.Observacao, ag.MotivoCancelamento, ag.SolicitacaoAtendimentoId));
    }

    public async Task<Result<List<AgendamentoDto>>> ListarAsync(Guid? profissionalId, DateTime? de, DateTime? ate, CancellationToken ct = default)
    {
        var clinicaId = _user.ClinicaId!.Value;
        var query = _db.Agendamentos.AsNoTracking().Where(a => a.ClinicaId == clinicaId);

        if (profissionalId is { } pid) query = query.Where(a => a.UsuarioProfissionalId == pid);
        if (de is { } d) query = query.Where(a => a.Inicio >= ParaUtc(d));
        if (ate is { } a2) query = query.Where(a => a.Inicio <= ParaUtc(a2));

        var itens = await query
            .OrderBy(a => a.Inicio)
            .Select(a => new AgendamentoDto(
                a.Id, a.PacienteId, a.Paciente!.Nome,
                a.UsuarioProfissionalId, a.UsuarioProfissional!.Nome,
                a.ProcedimentoId, a.Procedimento!.Nome,
                a.Inicio, a.Fim, a.Status, a.ValorCobrado, a.Observacao, a.MotivoCancelamento,
                a.SolicitacaoAtendimentoId))
            .ToListAsync(ct);

        return Result.Ok(itens);
    }

    public async Task<Result> AlterarStatusAsync(Guid id, AlterarStatusRequest req, CancellationToken ct = default)
    {
        var ag = await _db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ClinicaId == _user.ClinicaId, ct);
        if (ag is null) return Result.Falha("Agendamento não encontrado.", ErrorType.NotFound);
        if (ag.Status != StatusAgendamento.Agendado)
            return Result.Falha($"Agendamento já está '{ag.Status}' e não pode mudar de estado.", ErrorType.Conflict);

        if (req.Status is not (StatusAgendamento.Realizado or StatusAgendamento.Faltou or StatusAgendamento.Cancelado))
            return Result.Falha("Transição de status inválida.");
        if (req.Status == StatusAgendamento.Cancelado && string.IsNullOrWhiteSpace(req.MotivoCancelamento))
            return Result.Falha("Informe o motivo do cancelamento.");

        ag.Status = req.Status;
        if (req.Status == StatusAgendamento.Cancelado) ag.MotivoCancelamento = req.MotivoCancelamento;
        await _db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<AgendamentoDto>> AtualizarValorAsync(Guid id, decimal valor, CancellationToken ct = default)
    {
        if (valor < 0) return Result.Falha<AgendamentoDto>("Valor não pode ser negativo.");
        var ag = await _db.Agendamentos
            .Include(a => a.Paciente).Include(a => a.UsuarioProfissional).Include(a => a.Procedimento)
            .FirstOrDefaultAsync(a => a.Id == id && a.ClinicaId == _user.ClinicaId, ct);
        if (ag is null) return Result.Falha<AgendamentoDto>("Agendamento não encontrado.", ErrorType.NotFound);

        ag.ValorCobrado = valor;
        await _db.SaveChangesAsync(ct);
        return Result.Ok(new AgendamentoDto(
            ag.Id, ag.PacienteId, ag.Paciente!.Nome, ag.UsuarioProfissionalId, ag.UsuarioProfissional!.Nome,
            ag.ProcedimentoId, ag.Procedimento!.Nome, ag.Inicio, ag.Fim, ag.Status,
            ag.ValorCobrado, ag.Observacao, ag.MotivoCancelamento, ag.SolicitacaoAtendimentoId));
    }

    /// <summary>Normaliza para UTC (assume UTC quando o Kind não é informado).</summary>
    private static DateTime ParaUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };
}
