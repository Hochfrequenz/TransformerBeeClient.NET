using EDILibrary;

namespace TransformerBeeClient.Model;

/// <summary>
/// The request to convert edifact to bo4e
/// </summary>
internal class EdifactToBo4eRequest
{
    // internally we only use system.text, no newtonsoft

    /// <summary>
    /// the edifact as plain string
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("EDI")]
    public string Edifact { get; set; }

    /// <summary>
    /// the format version to use
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("FormatPackage")]
    public EdifactFormatVersion FormatVersion { get; set; }

    /// <summary>
    /// legacy for MP. can be false by default
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("UseMap")]
    public bool UseMap { get; set; } = false;
}
