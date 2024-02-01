namespace TransformerBeeClient.Model;

/// <summary>
/// The response to a <see cref="Bo4eTransactionToEdifactRequest"/>
/// </summary>
internal class Bo4eTransactionToEdifactResponse
{
    // internally we only use system.text, no newtonsoft

    /// <summary>
    /// the edifact as plain string
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("EDI")]
    public string? Edifact { get; set; }
}
