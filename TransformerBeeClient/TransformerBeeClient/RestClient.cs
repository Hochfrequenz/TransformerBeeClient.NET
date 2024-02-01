using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EDILibrary;
using TransformerBeeClient.Model;

namespace TransformerBeeClient;

/// <summary>
/// a client for the transformer.bee REST API
/// </summary>
public class TransformerBeeRestClient : ICanConvertToBo4e, ICanConvertToEdifact
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Provide the constructor with a http client factory.
    /// It will create a client from said factory and use the <paramref name="clientName"/> for that.
    /// </summary>
    /// <param name="httpClientFactory">factory to create the http client from</param>
    /// <param name="clientName">name used to create the client</param>
    /// <remarks>Find the OpenAPI Spec here: https://transformerstage.utilibee.io/swagger/index.html</remarks>
    public TransformerBeeRestClient(IHttpClientFactory httpClientFactory, string clientName = "TransformerBee")
    {
        _httpClient = httpClientFactory.CreateClient(clientName);
        if (_httpClient.BaseAddress == null)
        {
            throw new ArgumentNullException(nameof(httpClientFactory), $"The http client factory must provide a base address for the client with name '{clientName}'");
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
        var uriBuilder = new UriBuilder(_httpClient!.BaseAddress)
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
        var uriBuilder = new UriBuilder(_httpClient!.BaseAddress)
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
        var httpResponse = await _httpClient.PostAsync(convertUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));
        if (!httpResponse.IsSuccessStatusCode)
        {
            // e.g. 401
            throw new HttpRequestException($"Could not convert {edifact} to BO4E. Status code: {httpResponse.StatusCode}");
        }
        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var bo4eResponse = JsonSerializer.Deserialize<EdifactToBo4eResponse>(responseContent, _jsonSerializerOptions);
        // todo: handle the case that the deserialization fails and bo4eResponse is null
        var unescapedJson = bo4eResponse!.Bo4eJsonString.Unescape();
        var result = JsonSerializer.Deserialize<List<Marktnachricht>>(unescapedJson, _jsonSerializerOptions);
        // todo: handle the case that the deserialization fails and result is null
        return result;
    }

    public async Task<string> ConvertToEdifact(BOneyComb boneyComb, EdifactFormatVersion formatVersion)
    {
        if (boneyComb is null)
        {
            throw new ArgumentNullException(nameof(boneyComb));
        }
        var uriBuilder = new UriBuilder(_httpClient!.BaseAddress)
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
        var httpResponse = await _httpClient.PostAsync(convertUrl, new StringContent(requestJson, Encoding.UTF8, "application/json"));
        if (!httpResponse.IsSuccessStatusCode)
        {
            // e.g. 401
            throw new HttpRequestException($"Could not convert to EDIFACT; Status code: {httpResponse.StatusCode}");
        }
        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        var responseBody = JsonSerializer.Deserialize<Bo4eTransactionToEdifactResponse>(responseContent, _jsonSerializerOptions);
        return responseBody.Edifact;
    }
}
