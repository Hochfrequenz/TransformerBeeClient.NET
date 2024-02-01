using EDILibrary;
using FluentAssertions;
using TransformerBeeClient.Model;
using Xunit;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// Tests that a edifact can be converted to bo4e
/// </summary>
public class EdifactToBo4eTests : IClassFixture<ClientFixture>
{
    private readonly ClientFixture _client;

    private readonly ITransformerBeeAuthenticationProvider _authenticationProvider;

    public EdifactToBo4eTests(ClientFixture clientFixture)
    {
        _client = clientFixture;
        _authenticationProvider = clientFixture.AuthenticationProvider;
    }

    [Fact]
    public async Task Edifact_Can_Be_Converted_To_Bo4e()
    {
        var httpClientFactory = _client.HttpClientFactory;
        ICanConvertToBo4e client = new TransformerBeeRestClient(httpClientFactory, _authenticationProvider);
        var edifactString = await File.ReadAllTextAsync("TestEdifacts/FV2310/55001.edi");
        var result = await client.ConvertToBo4e(edifactString, EdifactFormatVersion.FV2310);
        result.Should().BeOfType<List<Marktnachricht>>();
        result.Single().Transaktionen.Should().NotBeNullOrEmpty();
    }
}

/// <summary>
/// those tests only run if the environment variables are set up correctly
/// </summary>
public class EdifactToBo4eTestsWithAuthentication : IClassFixture<GithubActionClientFixture>
{
    private readonly GithubActionClientFixture _client;

    private readonly ITransformerBeeAuthenticationProvider? _authenticationProvider;

    public EdifactToBo4eTestsWithAuthentication(GithubActionClientFixture clientFixture)
    {
        _client = clientFixture;
        _authenticationProvider = clientFixture.AuthenticationProvider;
    }

    [Fact]
    public async Task Edifact_Can_Be_Converted_To_Bo4e_With_Authentication()
    {
        if (_authenticationProvider is null)
        {
            await Console.Out.WriteLineAsync("Skipping tests because no client id or client secret is available");
            return;
        }
        var httpClientFactory = _client.HttpClientFactory;
        ICanConvertToBo4e client = new TransformerBeeRestClient(httpClientFactory, _authenticationProvider);
        var edifactString = await File.ReadAllTextAsync("TestEdifacts/FV2310/55001.edi");
        var result = await client.ConvertToBo4e(edifactString, EdifactFormatVersion.FV2310);
        result.Should().BeOfType<List<Marktnachricht>>();
        result.Single().Transaktionen.Should().NotBeNullOrEmpty();
    }
}
