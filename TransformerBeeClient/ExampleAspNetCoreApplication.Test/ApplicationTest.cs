using System.Net;
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
        var response = await client.GetAsync("/talkToTransformerBee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var bo4e = JsonSerializer.Deserialize<BOneyComb>(content, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        bo4e.Should().NotBeNull();
    }
}
