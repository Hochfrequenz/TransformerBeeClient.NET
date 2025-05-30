using EDILibrary;
using TransformerBeeClient.Model;

namespace TransformerBeeClient;

/// <summary>
/// Interface of all the things that can convert EDIFACT to BO4E
/// </summary>
/// <remarks>This will be useful if you want to mock the client elsewhere</remarks>
public interface ICanConvertToBo4e
{
    /// <summary>
    /// convert an edifact to BO4E
    /// </summary>
    /// <param name="edifact">edifact message as string</param>
    /// <param name="formatVersion"><see cref="EdifactFormatVersion"/></param>
    /// <returns><see cref="Marktnachricht"/></returns>
    public Task<List<Marktnachricht>> ConvertToBo4e(
        string edifact,
        EdifactFormatVersion formatVersion
    );
}

/// <summary>
/// Interface of all the things that can convert EDIFACT to BO4E
/// </summary>
/// <remarks>This will be useful if you want to mock the client elsewhere</remarks>
public interface ICanConvertToEdifact
{
    /// <summary>
    /// convert bo4e to edifact
    /// </summary>
    /// <param name="boneyComb">transaktion/Geschäftsvorfall as boneycomb</param>
    /// <param name="formatVersion"><see cref="EdifactFormatVersion"/></param>
    /// <returns>the edifact as plain string</returns>
    public Task<string> ConvertToEdifact(BOneyComb boneyComb, EdifactFormatVersion formatVersion);
}

/// <summary>
/// Can provide information on whether you need to authenticate against transformer.bee and how
/// </summary>
public interface ITransformerBeeAuthenticator
{
    /// <summary>
    /// returns true iff the client should use authentication
    /// </summary>
    /// <returns></returns>
    public bool UseAuthentication();

    /// <summary>
    /// provides the token to authenticate against transformer.bee (if and only if <see cref="UseAuthentication"/> is true)
    /// </summary>
    /// <returns></returns>
    public Task<string> Authenticate(HttpClient client);
}
