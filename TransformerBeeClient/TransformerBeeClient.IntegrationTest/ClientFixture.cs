using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// A fixture that sets up the http client factory and an injectable service collection
/// </summary>
public class ClientFixture : IClassFixture<ClientFixture>
{
    public readonly IHttpClientFactory HttpClientFactory;

    public readonly ServiceCollection ServiceCollection;

    public ClientFixture()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("TransformerBee", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5021"); // Check docker-compose.yml
        });
        var serviceProvider = services.BuildServiceProvider();
        ServiceCollection = services;
        HttpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
    }
}
