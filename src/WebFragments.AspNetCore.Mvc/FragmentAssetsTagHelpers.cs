using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Net; // For WebUtility

namespace WebFragments.AspNetCore.Mvc;

[HtmlTargetElement("web-fragment-styles")]
public class FragmentStylesTagHelper : TagHelper
{
    private readonly IFragmentAssetCollector _assetCollector;
    private readonly ILogger<FragmentStylesTagHelper> _logger;

    public FragmentStylesTagHelper(IFragmentAssetCollector assetCollector, ILogger<FragmentStylesTagHelper> logger)
    {
        _assetCollector = assetCollector;
        _logger = logger;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        var cssLinks = _assetCollector.GetCssLinks().ToList();
        var sb = new StringBuilder();
        foreach (var cssUrl in cssLinks)
        {
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{WebUtility.HtmlEncode(cssUrl)}\" />");
        }
        output.Content.SetHtmlContent(sb.ToString());
        _logger.LogDebug("Rendered {CssCount} CSS links for web fragments.", cssLinks.Count);
    }
}

[HtmlTargetElement("web-fragment-scripts")]
public class FragmentScriptsTagHelper(IFragmentAssetCollector assetCollector, ILogger<FragmentScriptsTagHelper> logger)
    : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        var jsScripts = assetCollector.GetJsScripts().ToList();
        var sb = new StringBuilder();
        foreach (var jsUrl in jsScripts)
        {
            sb.AppendLine($"<script src=\"{WebUtility.HtmlEncode(jsUrl)}\"></script>");
        }
        output.Content.SetHtmlContent(sb.ToString());
        logger.LogDebug("Rendered {JsCount} JS scripts for web fragments.", jsScripts.Count);
    }
}
