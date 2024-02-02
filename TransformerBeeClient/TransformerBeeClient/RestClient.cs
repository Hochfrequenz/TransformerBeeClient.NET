using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDILibrary;
using TransformerBeeClient.Model;
using System.Net;
using System.Net.Http.Headers;

namespace TransformerBeeClient;

/// <summary>
/// a client for the transformer.bee REST API
/// </summary>
public class TransformerBeeRestClient : ICanConvertToBo4e, ICanConvertToEdifact
{
    private readonly ITransformerBeeAuthenticator _authenticator;
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
    /// <param name="authenticator">something that tells you whether and how you need to authenticate yourself at transformer.bee</param>
    /// <param name="httpClientName">name used to create the client</param>
    /// <remarks>Find the OpenAPI Spec here: https://transformerstage.utilibee.io/swagger/index.html</remarks>
    public TransformerBeeRestClient(IHttpClientFactory httpClientFactory, ITransformerBeeAuthenticator authenticator, string httpClientName = "TransformerBee")
    {
        _httpClient = httpClientFactory.CreateClient(httpClientName);
        if (_httpClient.BaseAddress == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory), $"The http client factory must provide a base address for the client with name '{httpClientName}'");
        }

        _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
    }

    /// <summary>
    /// Make sure that the client is authenticated, if necessary
    /// </summary>
    private async Task EnsureAuthentication()
    {
        if (_authenticator.UseAuthentication())
        {
            var token = await _authenticator.Authenticate(_httpClient);
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
        // note that this is available without any authentication
        // see e.g. http://transformerstage.utilibee.io/version
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
            var errorContent = await httpResponse.Content.ReadAsStringAsync();
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && !_authenticator.UseAuthentication())
            {
                throw new AuthenticationException($"Did you correctly set up the {nameof(ITransformerBeeAuthenticator)}?");
            }

            throw new HttpRequestException($"Could not convert {edifact} to BO4E. Status code: {httpResponse.StatusCode} / {errorContent}");
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
            var errorContent = await httpResponse.Content.ReadAsStringAsync();
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized && !_authenticator.UseAuthentication())
            {
                throw new AuthenticationException($"Did you correctly set up the {nameof(ITransformerBeeAuthenticator)}?");
            }

            throw new HttpRequestException($"Could not convert to EDIFACT; Status code: {httpResponse.StatusCode} / {errorContent}");
        }

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        // todo: ensure that the deserialization does not fail and the response is not empty
        var responseBody = JsonSerializer.Deserialize<Bo4eTransactionToEdifactResponse>(responseContent!, _jsonSerializerOptions);
        // todo: handle case that deserialization fails and responseBody is null
        return responseBody!.Edifact!;
    }
}
