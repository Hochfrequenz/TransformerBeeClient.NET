namespace TransformerBeeClient;

using System.Security.Authentication;
using IdentityModel.Client;
using System.IdentityModel.Tokens.Jwt;

/// <summary>
/// a <see cref="ITransformerBeeAuthenticationProvider"/> that is based on a client secret and a client id
/// </summary>
public class ClientIdClientSecretAuthenticationProvider : ITransformerBeeAuthenticationProvider
{
    protected const string Auth0Domain = "https://hochfrequenz.eu.auth0.com";
    protected const string TransformerBeeScope = "client:default";
    protected const string TransformerAudience = "https://transformer.bee";

    /// <summary>
    /// JWT token
    /// </summary>
    private string? _token;

    private readonly string? _clientSecret;
    private readonly string? _clientId;

    /// <summary>
    /// true iff the client should use authentication
    /// </summary>
    private readonly bool _useAuthentication;

    /// <summary>
    /// <inheritdoc cref="ITransformerBeeAuthenticationProvider.UseAuthentication"/>
    /// </summary>
    public bool UseAuthentication() => _useAuthentication;

    /// <summary>
    /// prevents us from requesting multiple tokens at the same time
    /// </summary>
    private readonly SemaphoreSlim _tokenRequestSemaphore = new(1, 1);

    /// <summary>A <see cref="ITransformerBeeAuthenticationProvider"/> that is based on a client secret and a client id</summary>
    /// <param name="clientId">OAuth2 client secret (ask Hochfrequenz)</param>
    /// <param name="clientSecret">OAuth2 client ID (ask Hochfrequenz)</param>
    public ClientIdClientSecretAuthenticationProvider(string? clientId, string? clientSecret)
    {
        if (!string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentNullException(nameof(clientSecret), $"If you provide a {nameof(clientId)} you must also provide a {nameof(clientSecret)}");
        }

        if (!string.IsNullOrWhiteSpace(clientSecret) && string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentNullException(nameof(clientId), $"If you provide a {nameof(clientSecret)} you must also provide a {nameof(clientId)}");
        }

        _useAuthentication = !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
        if (_useAuthentication)
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
    private static async Task<DiscoveryDocumentResponse> GetDiscoveryDocument(HttpClient client)
    {
        var discoveryDocument = await client.GetDiscoveryDocumentAsync(Auth0Domain);
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
    public async Task<string> GetTokenAsync(HttpClient client)
    {
        using (_tokenRequestSemaphore)
        {
            if (!UseAuthentication())
            {
                throw new AuthenticationException("You must provide a client id and a client secret to the constructor to authenticate with the transformer bee");
            }

            if (!string.IsNullOrWhiteSpace(_token) && !TokenNeedsToBeRefreshed(_token))
            {
                return _token;
            }

            var discoveryDocument = await GetDiscoveryDocument(client);
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = _clientId!,
                ClientSecret = _clientSecret!,
                GrantType = "client_credentials",
                Scope = TransformerBeeScope,
            };
            tokenRequest.Parameters.AddOptional("audience", TransformerAudience);

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(tokenRequest);
            if (tokenResponse.IsError)
            {
                throw new AuthenticationException($"Could not get token from auth0: {tokenResponse.Error}");
            }

            _token = tokenResponse.AccessToken!;
        }

        return _token;
    }
}
