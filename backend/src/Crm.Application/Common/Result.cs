namespace Crm.Application.Common;

/// <summary>Tipo de erro, para o Api traduzir em status HTTP adequado.</summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}

/// <summary>Resultado de operação sem payload.</summary>
public class Result
{
    public bool Sucesso { get; }
    public string? Erro { get; }
    public ErrorType TipoErro { get; }

    protected Result(bool sucesso, string? erro, ErrorType tipoErro)
    {
        Sucesso = sucesso;
        Erro = erro;
        TipoErro = tipoErro;
    }

    public static Result Ok() => new(true, null, ErrorType.None);
    public static Result Falha(string erro, ErrorType tipo = ErrorType.Validation) => new(false, erro, tipo);

    public static Result<T> Ok<T>(T valor) => Result<T>.Ok(valor);
    public static Result<T> Falha<T>(string erro, ErrorType tipo = ErrorType.Validation) => Result<T>.Falha(erro, tipo);
}

/// <summary>Resultado de operação com payload.</summary>
public class Result<T> : Result
{
    public T? Valor { get; }

    private Result(bool sucesso, T? valor, string? erro, ErrorType tipoErro)
        : base(sucesso, erro, tipoErro) => Valor = valor;

    public static Result<T> Ok(T valor) => new(true, valor, null, ErrorType.None);
    public static new Result<T> Falha(string erro, ErrorType tipo = ErrorType.Validation) => new(false, default, erro, tipo);
}
