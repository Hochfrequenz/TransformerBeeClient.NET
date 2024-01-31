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
    public Task<List<Marktnachricht>> ConvertToBo4e(string edifact, EdifactFormatVersion formatVersion);
}
