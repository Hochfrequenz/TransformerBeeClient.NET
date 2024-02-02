using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TransformerBeeClient.UnitTest;

public class ConnectionTests
{
    [Fact]
    public void IsAvailable_Throws_ArgumentNullException_If_BaseAddress_Is_Not_Configured()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("TransformerBee", client =>
        {
            client.BaseAddress = null;
        });
        var serviceProvider = services.BuildServiceProvider();
        var instantiateClient = () => new TransformerBeeRestClient(serviceProvider.GetService<IHttpClientFactory>(), new NoAuthenticator());
        instantiateClient.Should().Throw<ArgumentNullException>();
    }
}
