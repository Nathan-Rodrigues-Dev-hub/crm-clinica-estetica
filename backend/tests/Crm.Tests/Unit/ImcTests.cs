using Crm.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Crm.Tests.Unit;

public class ImcTests
{
    [Fact]
    public void RecalcularImc_calcula_peso_sobre_altura_ao_quadrado()
    {
        var a = new Anamnese { Peso = 70m, Altura = 1.75m };
        a.RecalcularImc();
        // 70 / (1.75^2) = 22.857... → 22.86
        a.Imc.Should().Be(22.86m);
    }

    [Theory]
    [InlineData(null, 1.70)]
    [InlineData(60d, null)]
    [InlineData(0d, 1.70)]
    [InlineData(60d, 0d)]
    public void RecalcularImc_sem_dados_validos_fica_nulo(double? peso, double? altura)
    {
        var a = new Anamnese
        {
            Peso = peso is null ? null : (decimal)peso.Value,
            Altura = altura is null ? null : (decimal)altura.Value
        };
        a.RecalcularImc();
        a.Imc.Should().BeNull();
    }
}
