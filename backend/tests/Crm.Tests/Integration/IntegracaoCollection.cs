using Crm.Tests.Infra;
using Xunit;

namespace Crm.Tests.Integration;

/// <summary>Compartilha uma única instância da API (e do banco de teste) entre os testes de integração.</summary>
[CollectionDefinition("Integração")]
public class IntegracaoCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
