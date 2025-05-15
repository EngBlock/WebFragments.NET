namespace WebFragments.Core;

/// <summary>
/// Holds the extracted data from a web fragment.
/// </summary>
public class FragmentData
{
    public string BodyContent { get; set; } = string.Empty;
    public List<string> CssLinks { get; set; } = new List<string>();
    public List<string> JsScripts { get; set; } = new List<string>();
}