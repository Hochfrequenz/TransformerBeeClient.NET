using System.Text.Json.Serialization;
using BO4E.BO;
using Newtonsoft.Json;

namespace TransformerBeeClient.Model;

/// <summary>
/// BOneyComb is a data structure that represents a "transaction" in the marktkommunikation.
/// 1 transaction = 1 Geschäftsvorfall
/// </summary>
public class BOneyComb
{
    /// <summary>
    /// the business objects
    /// </summary>
    [JsonPropertyName("stammdaten")] // we want to support both System.Text and Newtonsoft as long as BO4E.net does so
    [JsonProperty(PropertyName = "stammdaten")]
    public List<BusinessObject>? Stammdaten { get; set; }

    /// <summary>
    /// Transaktionsdaten are metadata related to the Marktprozess and are not related to a specific Business object.
    /// </summary>
    [JsonPropertyName("transaktionsdaten")] // we want to support both System.Text and Newtonsoft as long as BO4E.net does so
    [JsonProperty(PropertyName = "transaktionsdaten")]
    public Dictionary<string, string>? Transaktionsdaten { get; set; }
}
