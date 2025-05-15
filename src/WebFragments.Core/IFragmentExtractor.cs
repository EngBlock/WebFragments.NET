namespace WebFragments.Core;

/// <summary>
/// Defines the contract for extracting data from a web fragment.
/// </summary>
public interface IFragmentExtractor
{
    /// <summary>
    /// Asynchronously extracts CSS links, JS script sources, and body content from the given URL.
    /// </summary>
    /// <param name="fragmentUrl">The URL of the HTML fragment to process.</param>
    /// <param name="bodyContentSelector">
    /// Optional XPath selector to extract a specific part of the fragment's body.
    /// If null or empty, the entire content of the body tag is used.
    /// </param>
    /// <returns>A <see cref="FragmentData"/> object or null if extraction fails.</returns>
    Task<FragmentData?> ExtractFragmentDataAsync(
        string fragmentUrl,
        string? bodyContentSelector = null
    );
}