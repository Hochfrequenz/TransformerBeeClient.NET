using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// A fixture that sets up the http client factory and an injectable service collection.
/// It's thought to be used in the Github action.
/// </summary>
public class GithubActionClientFixture : IClassFixture<GithubActionClientFixture>
{
    public readonly IHttpClientFactory? HttpClientFactory;

    public readonly ServiceCollection? ServiceCollection;

    public readonly ITransformerBeeAuthenticationProvider? AuthenticationProvider;

    public GithubActionClientFixture()
    {
        var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            Console.Out.WriteLine("Skipping tests because no client id or client secret is available");
            return;
        }

        var services = new ServiceCollection();
        services.AddHttpClient("TransformerBee", client => { client.BaseAddress = new Uri("http://transformerstage.utilibee.io"); });
        var serviceProvider = services.BuildServiceProvider();
        ServiceCollection = services;
        HttpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        AuthenticationProvider = new ClientIdClientSecretAuthenticationProvider(clientId: clientId, clientSecret: clientSecret);
    }
}
