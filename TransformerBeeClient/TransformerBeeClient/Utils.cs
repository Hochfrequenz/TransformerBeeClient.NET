namespace TransformerBeeClient;

/// <summary>
/// Utility extension methods
/// </summary>
internal static class Utils
{
    /// <summary>
    /// revert the escaping of the converter
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static string? Unescape(this string? s)
    {
        return s?.Replace("\\n", "\n");
    }
}
