using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TransformerBeeClient.Model;

/// <summary>
/// A marktnachricht contains of 1 or more <see cref="BOneyComb"/>s
/// </summary>
/// <remarks>E.g. a UTILMD message with more than 1 IDE contains multiple transactions</remarks>
public class Marktnachricht
{
    /// <summary>
    /// the UNH (interchange header) of the message
    /// </summary>
    [JsonPropertyName("UNH")] // we want to support both System.Text and Newtonsoft as long as BO4E.net does so
    [JsonProperty(PropertyName = "UNH")]
    public string? UNH { get; set; }

    /// <summary>
    /// One marktnachricht contains at least 1 transaction aka BOneyComb
    /// </summary>
    [JsonPropertyName("transaktionen")] // we want to support both System.Text and Newtonsoft as long as BO4E.net does so
    [JsonProperty(PropertyName = "transaktionen")]
    public List<BOneyComb>? Transaktionen { get; set; }

    /// <summary>
    /// Nachrichtendaten are similar to <see cref="BOneyComb.Transaktionsdaten"/> but are not 100% identical.
    /// </summary>
    [JsonPropertyName("nachrichtendaten")] // we want to support both System.Text and Newtonsoft as long as BO4E.net does so
    [JsonProperty(PropertyName = "nachrichtendaten")]
    public Dictionary<string, string>? Nachrichtendaten { get; set; }
}
