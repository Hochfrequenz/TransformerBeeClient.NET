using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TransformerBeeClient.Model;

namespace ExampleAspNetCoreApplication.Test;

public class ApplicationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;

    public ApplicationTest(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
    }

    [Fact]
    public async Task Test_That_Setup_Works_As_Designed()
    {
        var client = Factory.CreateDefaultClient();
        var bo4e = await client.GetFromJsonAsync<BOneyComb>("/talkToTransformerBee", new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        bo4e.Should().NotBeNull();
    }
}
