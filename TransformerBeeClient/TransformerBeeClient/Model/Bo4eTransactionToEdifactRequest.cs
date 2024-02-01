using EDILibrary;

namespace TransformerBeeClient.Model;

/// <summary>
/// The request to convert a single transaction/BOneyComb to edifact
/// </summary>
internal class Bo4eTransactionToEdifactRequest
{
    // internally we only use system.text, no newtonsoft

    /// <summary>
    /// the BOneyComb as json string 
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("BO4E")]
    public string Bo4eJsonString { get; set; }

    /// <summary>
    /// the format version to use
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("FormatPackage")]
    public EdifactFormatVersion FormatVersion { get; set; }
}
