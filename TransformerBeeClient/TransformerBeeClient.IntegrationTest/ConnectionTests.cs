using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// Tests that a connection to the API can be established
/// </summary>
public class ConnectionTests : IClassFixture<ClientFixture>
{

    private readonly ClientFixture _client;
    private readonly ITransformerBeeAuthenticator _authenticator;

    public ConnectionTests(ClientFixture clientFixture)
    {
        _client = clientFixture;
        _authenticator = clientFixture.Authenticator;
    }

    [Fact]
    public async Task IsAvailable_Returns_True_If_Service_Is_Available()
    {
        var httpClientFactory = _client.HttpClientFactory;
        var client = new TransformerBeeRestClient(httpClientFactory, _authenticator);
        var result = await client.IsAvailable();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailable_Throws_Exception_If_Host_Is_Unavailable()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("TransformerBee", client =>
        {
            client.BaseAddress = new Uri("http://localhost:1234"); // <-- no service running under this address
        });
        var serviceProvider = services.BuildServiceProvider();
        var client = new TransformerBeeRestClient(serviceProvider.GetService<IHttpClientFactory>(), _authenticator);
        var checkIfIsAvailable = async () => await client.IsAvailable();
        await checkIfIsAvailable.Should().ThrowAsync<HttpRequestException>();
    }
}
