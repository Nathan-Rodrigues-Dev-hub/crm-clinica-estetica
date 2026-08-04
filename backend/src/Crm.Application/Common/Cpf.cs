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

    /// <summary>Validação simples de tamanho (11 dígitos).</summary>
    public static bool TamanhoValido(string? cpfNormalizado) =>
        cpfNormalizado is { Length: 11 };

    /// <summary>
    /// Validação completa: 11 dígitos + dígitos verificadores corretos.
    /// Rejeita sequências repetidas (000..., 111...) que passam no cálculo mas são inválidas.
    /// </summary>
    public static bool Valido(string? cpfNormalizado)
    {
        if (!TamanhoValido(cpfNormalizado)) return false;
        var cpf = cpfNormalizado!;

        // Todos os dígitos iguais (ex.: 111.111.111-11) são formalmente inválidos.
        if (cpf.Distinct().Count() == 1) return false;

        return DigitoVerificador(cpf, 9) == cpf[9] - '0'
            && DigitoVerificador(cpf, 10) == cpf[10] - '0';
    }

    /// <summary>Calcula o dígito verificador na posição informada (9 = 1º DV, 10 = 2º DV).</summary>
    private static int DigitoVerificador(string cpf, int posicao)
    {
        var soma = 0;
        var peso = posicao + 1; // 10 para o 1º DV, 11 para o 2º DV
        for (var i = 0; i < posicao; i++)
            soma += (cpf[i] - '0') * peso--;

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
