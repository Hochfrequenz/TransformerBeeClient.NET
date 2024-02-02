namespace TransformerBeeClient;

/// <summary>
/// a <see cref="ITransformerBeeAuthenticator"/> stub that just indicates, we don't need to authenticate against transformer.bee, because we're e.g. in the same network
/// </summary>
public class NoAuthenticator : ITransformerBeeAuthenticator
{
    public bool UseAuthentication() => false;

    public Task<string> Authenticate(HttpClient client)
    {
        throw new NotImplementedException("This must never be called, because we don't use authentication.");
    }
}
