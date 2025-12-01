using System.Text.Json;
using System.Text.Json.Serialization;
using EDILibrary;
using AwesomeAssertions;
using TransformerBeeClient.Model;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// Tests that a bo4e can be converted to edifact
/// </summary>
public class Bo4eToEdifactTests : IClassFixture<ClientFixture>
{
    private readonly ClientFixture _client;
    private readonly ITransformerBeeAuthenticator _authenticator;

    public Bo4eToEdifactTests(ClientFixture clientFixture)
    {
        _client = clientFixture;
        _authenticator = clientFixture.Authenticator;
    }

    [Fact]
    public async Task BOneyComb_Can_Be_Converted_To_Edifact()
    {
        var httpClientFactory = _client.HttpClientFactory;
        ICanConvertToEdifact client = new TransformerBeeRestClient(
            httpClientFactory,
            _authenticator
        );
        var boneyCombString = await File.ReadAllTextAsync("TestEdifacts/FV2310/55001.json");
        var deserializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };
        var boneyComb = System.Text.Json.JsonSerializer.Deserialize<BOneyComb>(
            boneyCombString,
            deserializerOptions
        );
        boneyComb.Should().NotBeNull();
        var result = await client.ConvertToEdifact(boneyComb, EdifactFormatVersion.FV2310);
        result.Should().BeOfType<string>().And.StartWith("UNB+UNOC");
    }
}
