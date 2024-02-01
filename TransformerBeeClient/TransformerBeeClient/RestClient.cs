using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDILibrary;
using TransformerBeeClient.Model;
using IdentityModel.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace TransformerBeeClient;

/// <summary>
/// a client for the transformer.bee REST API
/// </summary>
public class TransformerBeeRestClient : ICanConvertToBo4e, ICanConvertToEdifact
{
    protected const string Auth0Domain = "https://hochfrequenz.eu.auth0.com";
    protected const string TransformerBeeScope = "client:default";
    protected const string TransformerAudience = "https://transformer.bee";

    /// <summary>
    /// JWT token
    /// </summary>
    private string? _token;


    /// <summary>
    /// true iff the client should use authentication
    /// </summary>
    private bool UseAuthentication { get; }

    private readonly string? _clientSecret;
    private readonly string? _clientId;

    /// <summary>
    /// prevents us from requesting multiple tokens at the same time
    /// </summary>
    private readonly SemaphoreSlim _tokenRequestSemaphore = new(1, 1);


    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Provide the constructor with a http client factory.
    /// It will create a client from said factory and use the <paramref name="httpClientName"/> for that.
    /// </summary>
    /// <param name="httpClientFactory">factory to create the http client from</param>
    /// <param name="clientSecret">OAuth2 client ID</param>
    /// <param name="httpClientName">name used to create the client</param>
    /// <param name="clientId">OAuth2 client secret</param>
    /// <remarks>Find the OpenAPI Spec here: https://transformerstage.utilibee.io/swagger/index.html</remarks>
    public TransformerBeeRestClient(IHttpClientFactory httpClientFactory, string? clientId = null, string? clientSecret = null, string httpClientName = "TransformerBee")
    {
        _httpClient = httpClientFactory.CreateClient(httpClientName);
        if (_httpClient.BaseAddress == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory), $"The http client factory must provide a base address for the client with name '{httpClientName}'");
        }

        if (!string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentNullException(nameof(clientSecret), $"If you provide a {nameof(clientId)} you must also provide a {nameof(clientSecret)}");
        }

        if (!string.IsNullOrWhiteSpace(clientSecret) && string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), $"If you provide a {nameof(clientSecret)} you must also provide a {nameof(clientId)}");
        }

        UseAuthentication = !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
        if (UseAuthentication)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }
    }

    /// <summary>
    /// returns true if the <see cref="token"/> will expire in the next 5 minutes
    /// </summary>
    /// <param name="token">jwt</param>
    /// <returns>true if the token should be refreshed</returns>
    private static bool TokenNeedsToBeRefreshed(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null) return true; // Assume renewal if token cannot be read
        var expiry = jwtToken.ValidTo;
        // Consider a buffer to renew the token a bit before it actually expires
        var buffer = TimeSpan.FromMinutes(5);
        return DateTime.UtcNow.Add(buffer) >= expiry;
    }

    /// <summary>
    /// gets the oauth2 discovery document from the respective auth0 domain
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AuthenticationException"></exception>
    private async Task<DiscoveryDocumentResponse> GetDiscoveryDocument()
    {
        var discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync(Auth0Domain);
        if (discoveryDocument.IsError)
        {
            throw new AuthenticationException($"Could not get discovery document from auth0: {discoveryDocument.Error}");
        }

        return discoveryDocument;
    }

    /// <summary>
    /// fetch a token to talk to the transformer bee
    /// </summary>
    /// <returns>the token</returns>
    /// <exception cref="AuthenticationException">if something goes wrong</exception>
    private async Task<string> GetTokenAsync()
    {
        using (_tokenRequestSemaphore)
        {
            if (!UseAuthentication)
            {
                throw new AuthenticationException("You must provide a client id and a client secret to the constructor to authenticate with the transformer bee");
            }

            if (!string.IsNullOrWhiteSpace(_token) && !TokenNeedsToBeRefreshed(_token))
            {
                return _token;
            }

            var discoveryDocument = await GetDiscoveryDocument();
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = _clientId!,
                ClientSecret = _clientSecret!,
                GrantType = "client_credentials",
                Scope = TransformerBeeScope,
            };
            tokenRequest.Parameters.AddOptional("audience", TransformerAudience);

            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest);
            if (tokenResponse.IsError)
            {
                throw new AuthenticationException($"Could not get token from auth0: {tokenResponse.Error}");
            }

            _token = tokenResponse.AccessToken!;
        }

        return _token;
    }

    /// <summary>
    /// Make sure that the client is authenticated, if necessary
    /// </summary>
    private async Task EnsureAuthentication()
    {
        if (UseAuthentication)
        {
            var token = await GetTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// tests if transformer bee is available
    /// </summary>
    /// <remarks>
    /// Note that this does _not_ check if you're authenticated.
    /// The method will probably throw an <see cref="HttpRequestException"/> if the host cannot be found.
    /// </remarks>
    /// <returns>
    /// Returns true iff the transformer bee is available under the configured base address.
    /// </returns>
    public async Task<bool> IsAvailable()
    {
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = "/version"
        };

        var versionUrl = uriBuilder.Uri.AbsoluteUri;
        var response = await _httpClient.GetAsync(versionUrl);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// convert an edifact to BO4E
    /// </summary>
    /// <param name="edifact">edifact message as string</param>
    /// <param name="formatVersion"><see cref="EdifactFormatVersion"/></param>
    /// <returns><see cref="Marktnachricht"/></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<List<Marktnachricht>> ConvertToBo4e(string edifact, EdifactFormatVersion formatVersion)
    {
        if (string.IsNullOrWhiteSpace(edifact))
        {
            throw new ArgumentNullException(nameof(edifact));
        }

        var uriBuilder = new UriBuilder(_httpClient!.BaseAddress!)
        {
            Path = "/v1/transformer/EdiToBo4E"
        };

        var convertUrl = uriBuilder.Uri.AbsoluteUri;
        var request = new EdifactToBo4eRequest
        {
            Edifact = edifact,
            FormatVersion = formatVersion,
        };
        var requestJson = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        await EnsureAuthentication();
        var httpResponse = await _httpClient.PostAsync(convertUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));
        if (!httpResponse.IsSuccessStatusCode)
        {
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && !UseAuthentication)
            {
                throw new AuthenticationException("You have to provide client id and client secret to the constructor to authenticate yourself against transformer bee.");
            }

            throw new HttpRequestException($"Could not convert {edifact} to BO4E. Status code: {httpResponse.StatusCode}");
        }

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var bo4eResponse = JsonSerializer.Deserialize<EdifactToBo4eResponse>(responseContent, _jsonSerializerOptions);
        // todo: handle the case that the deserialization fails and bo4eResponse is null
        var unescapedJson = bo4eResponse!.Bo4eJsonString!.Unescape();
        var result = JsonSerializer.Deserialize<List<Marktnachricht>>(unescapedJson!, _jsonSerializerOptions);
        // todo: handle the case that the deserialization fails and result is null
        return result!;
    }

    public async Task<string> ConvertToEdifact(BOneyComb boneyComb, EdifactFormatVersion formatVersion)
    {
        if (boneyComb is null)
        {
            throw new ArgumentNullException(nameof(boneyComb));
        }

        var uriBuilder = new UriBuilder(_httpClient!.BaseAddress!)
        {
            Path = "/v1/transformer/Bo4ETransactionToEdi"
        };

        var convertUrl = uriBuilder.Uri.AbsoluteUri;
        var bo4eJsonString = JsonSerializer.Serialize(boneyComb, _jsonSerializerOptions);
        var request = new Bo4eTransactionToEdifactRequest
        {
            Bo4eJsonString = bo4eJsonString,
            FormatVersion = formatVersion,
        };
        var requestJson = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        await EnsureAuthentication();
        var httpResponse = await _httpClient.PostAsync(convertUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));
        if (!httpResponse.IsSuccessStatusCode)
        {
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && !UseAuthentication)
            {
                throw new AuthenticationException("You have to provide client id and client secret to the constructor to authenticate yourself against transformer bee.");
            }

            throw new HttpRequestException($"Could not convert to EDIFACT; Status code: {httpResponse.StatusCode}");
        }

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        // todo: ensure that the deserialization does not fail and the response is not empty
        var responseBody = JsonSerializer.Deserialize<Bo4eTransactionToEdifactResponse>(responseContent!, _jsonSerializerOptions);
        // todo: handle case that deserialization fails and responseBody is null
        return responseBody!.Edifact!;
    }
}
