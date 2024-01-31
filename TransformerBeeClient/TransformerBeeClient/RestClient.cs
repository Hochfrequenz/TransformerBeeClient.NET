namespace TransformerBeeClient;

/// <summary>
/// a client for the transformer.bee REST API
/// </summary>
public class TransformerBeeRestClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Provide the constructor with a http client factory.
    /// It will create a client from said factory and use the <paramref name="clientName"/> for that.
    /// </summary>
    /// <param name="httpClientFactory">factory to create the http client from</param>
    /// <param name="clientName">name used to create the client</param>
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
}
