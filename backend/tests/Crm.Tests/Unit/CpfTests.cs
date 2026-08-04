using Crm.Application.Common;
using FluentAssertions;
using Xunit;

namespace Crm.Tests.Unit;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")] // CPF válido conhecido
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void Valido_aceita_cpf_com_digito_verificador_correto(string cpf)
    {
        Cpf.Valido(Cpf.Normalizar(cpf)).Should().BeTrue();
    }

    [Theory]
    [InlineData("529.982.247-24")] // DV errado
    [InlineData("11111111111")]     // todos iguais
    [InlineData("00000000000")]
    [InlineData("123")]             // tamanho errado
    [InlineData("1234567890")]      // 10 dígitos
    public void Valido_rejeita_cpf_invalido(string cpf)
    {
        Cpf.Valido(Cpf.Normalizar(cpf)).Should().BeFalse();
    }

    [Fact]
    public void Normalizar_remove_mascara()
    {
        Cpf.Normalizar("529.982.247-25").Should().Be("52998224725");
        Cpf.Normalizar("   ").Should().BeNull();
        Cpf.Normalizar(null).Should().BeNull();
    }
}
