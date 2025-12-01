using EDILibrary;
using AwesomeAssertions;
using TransformerBeeClient.Model;
using Xunit;
using Xunit.Abstractions;

namespace TransformerBeeClient.IntegrationTest;

/// <summary>
/// Tests that a edifact can be converted to bo4e
/// </summary>
public class EdifactToBo4eTests : IClassFixture<ClientFixture>
{
    private readonly ClientFixture _client;

    private readonly ITransformerBeeAuthenticator _authenticator;

    public EdifactToBo4eTests(ClientFixture clientFixture)
    {
        _client = clientFixture;
        _authenticator = clientFixture.Authenticator;
    }

    [Fact]
    public async Task Edifact_Can_Be_Converted_To_Bo4e()
    {
        var httpClientFactory = _client.HttpClientFactory;
        ICanConvertToBo4e client = new TransformerBeeRestClient(httpClientFactory, _authenticator);
        var edifactString = await File.ReadAllTextAsync("TestEdifacts/FV2310/55001.edi");
        var result = await client.ConvertToBo4e(edifactString, EdifactFormatVersion.FV2310);
        result.Should().BeOfType<List<Marktnachricht>>();
        result.Single().Transaktionen.Should().NotBeNullOrEmpty();
    }
}

/// <summary>
/// those tests only run if the environment variables are set up correctly
/// </summary>
[Collection("authenticated")]
public class EdifactToBo4eTestsWithAuthentication : IClassFixture<GithubActionClientFixture>
{
    private readonly GithubActionClientFixture _client;

    private readonly ITransformerBeeAuthenticator? _authenticationProvider;

    ITestOutputHelper _output;

    public EdifactToBo4eTestsWithAuthentication(
        GithubActionClientFixture clientFixture,
        ITestOutputHelper output
    )
    {
        _client = clientFixture;
        _authenticationProvider = clientFixture.AuthenticationProvider;
        _output = output;
    }

    [SkippableFact]
    public async Task Edifact_Can_Be_Converted_To_Bo4e_With_Authentication()
    {
        if (_authenticationProvider is null)
        {
            _output.WriteLine("Skipping test because no authentication provider is available");
        }
        Skip.If(_authenticationProvider is null, "No authentication provider available");
        var httpClientFactory = _client.HttpClientFactory;
        ICanConvertToBo4e client = new TransformerBeeRestClient(
            httpClientFactory,
            _authenticationProvider
        );
        var edifactString = await File.ReadAllTextAsync("TestEdifacts/FV2310/55001.edi");
        var result = await client.ConvertToBo4e(edifactString, EdifactFormatVersion.FV2310);
        result.Should().BeOfType<List<Marktnachricht>>();
        result.Single().Transaktionen.Should().NotBeNullOrEmpty();
        _output.WriteLine("Successfully converted edifact to bo4e - with authentication!");
    }
}
