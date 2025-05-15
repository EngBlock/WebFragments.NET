using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebFragments.Core;

namespace WebFragments.AspNetCore.Mvc;

[HtmlTargetElement("web-fragment")]
public class WebFragmentTagHelper(
    IFragmentExtractor fragmentExtractor,
    IFragmentAssetCollector assetCollector,
    ILogger<WebFragmentTagHelper> logger)
    : TagHelper
{
    [HtmlAttributeName("source-url")]
    public string SourceUrl { get; set; } = string.Empty;

    [HtmlAttributeName("source-body-selector")]
    public string? SourceBodySelector { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrWhiteSpace(SourceUrl))
        {
            logger.LogWarning("WebFragmentTagHelper: SourceUrl is not set.");
            output.SuppressOutput();
            return;
        }
        logger.LogInformation("Processing web fragment from URL: {SourceUrl}", SourceUrl);

        var fragmentData = await fragmentExtractor.ExtractFragmentDataAsync(SourceUrl, SourceBodySelector);

        if (fragmentData == null)
        {
            logger.LogWarning("WebFragmentTagHelper: No data extracted from {SourceUrl}.", SourceUrl);
            output.SuppressOutput();
            return;
        }

        fragmentData.CssLinks.ForEach(assetCollector.AddCssLink);
        fragmentData.JsScripts.ForEach(assetCollector.AddJsScript);

        output.TagName = null; // Render no outer tag, just the content
        output.Content.SetHtmlContent(fragmentData.BodyContent);
        logger.LogDebug("Successfully processed web fragment from {SourceUrl}. Body length: {BodyLength}", SourceUrl, fragmentData.BodyContent.Length);
    }
}
