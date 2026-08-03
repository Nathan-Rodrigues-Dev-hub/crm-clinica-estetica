using System.Text.RegularExpressions;

namespace Crm.Application.Common;

public static partial class Cpf
{
    [GeneratedRegex(@"\D")]
    private static partial Regex NaoDigitos();

    /// <summary>Remove tudo que não for dígito. Retorna null se vazio.</summary>
    public static string? Normalizar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return null;
        var digitos = NaoDigitos().Replace(cpf, "");
        return digitos.Length == 0 ? null : digitos;
    }

    /// <summary>Validação simples de tamanho (11 dígitos). Não valida dígito verificador no MVP.</summary>
    public static bool TamanhoValido(string? cpfNormalizado) =>
        cpfNormalizado is { Length: 11 };
}
