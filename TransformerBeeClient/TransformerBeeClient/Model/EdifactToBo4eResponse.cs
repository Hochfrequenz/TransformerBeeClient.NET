using EDILibrary;

namespace TransformerBeeClient.Model;

/// <summary>
/// The response to a <see cref="EdifactToBo4eRequest"/>
/// </summary>
internal class EdifactToBo4eResponse
{
    // internally we only use system.text, no newtonsoft

    /// <summary>
    /// the bo4e as plain json string
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("BO4E")]
    public string? Bo4eJsonString { get; set; }

    /// <summary>
    /// the format version that was used
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("FormatPackage")]
    public EdifactFormatVersion FormatVersion { get; set; }
}
