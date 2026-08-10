namespace Crm.Domain.Enums;

/// <summary>Papel da equipe no sistema (autorização por papel).</summary>
public enum Papel
{
    Admin = 1,
    Profissional = 2,
    Recepcao = 3,
    /// <summary>Suporte/dono da plataforma — acima do Admin da clínica. Só é criado via seed do deploy, nunca pela tela.</summary>
    SuperAdmin = 4
}

/// <summary>Como o cadastro do paciente foi criado.</summary>
public enum OrigemPaciente
{
    /// <summary>A própria paciente preencheu o link tokenizado.</summary>
    AutoCadastroLink = 1,
    /// <summary>Cadastro feito manualmente pela recepção.</summary>
    Recepcao = 2
}

/// <summary>Ciclo de vida de uma solicitação de atendimento (triagem).</summary>
public enum StatusSolicitacao
{
    Recebida = 1,
    EmTriagem = 2,
    Agendada = 3,
    Recusada = 4
}

/// <summary>Ciclo de vida de um agendamento.</summary>
public enum StatusAgendamento
{
    Agendado = 1,
    Realizado = 2,
    Faltou = 3,
    Cancelado = 4
}

/// <summary>Etiqueta de uma mídia (foto/vídeo) do paciente.</summary>
public enum EtiquetaMidia
{
    Antes = 1,
    Depois = 2,
    Durante = 3,
    Documento = 4
}
