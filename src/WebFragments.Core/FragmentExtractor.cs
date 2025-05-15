using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace WebFragments.Core;

public class FragmentExtractor(
    IHttpClientFactory httpClientFactory,
    ILogger<FragmentExtractor> logger)
    : IFragmentExtractor
{
    public async Task<FragmentData?> ExtractFragmentDataAsync(
        string fragmentUrl,
        string? bodyContentSelector = null
    )
    {
        if (!Uri.TryCreate(
                fragmentUrl,
                UriKind.Absolute,
                out Uri? validatedUri
            ) ||
            (validatedUri.Scheme != Uri.UriSchemeHttp &&
             validatedUri.Scheme != Uri.UriSchemeHttps))
        {
            logger.LogError(
                "Invalid fragment URL: {FragmentUrl}",
                fragmentUrl
            );
            return null;
        }

        string htmlContent;
        try
        {
            var httpClient = httpClientFactory.CreateClient(
                "WebFragmentsClient"
            );
            htmlContent = await httpClient.GetStringAsync(validatedUri);
            logger.LogInformation(
                "Successfully fetched fragment from {ValidatedUri}",
                validatedUri
            );
        }
        catch (HttpRequestException e)
        {
            logger.LogError(
                e,
                "Error downloading fragment from {ValidatedUri}",
                validatedUri
            );
            return null;
        }

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        var fragmentData = new FragmentData();

        ExtractAssets(htmlDoc, validatedUri, fragmentData);
        ExtractBody(htmlDoc, bodyContentSelector, fragmentData, validatedUri);

        logger.LogDebug(
            "Extracted from {ValidatedUri}: {CssCount} CSS, {JsCount} JS, Body length: {BodyLength}",
            validatedUri,
            fragmentData.CssLinks.Count,
            fragmentData.JsScripts.Count,
            fragmentData.BodyContent.Length
        );
        return fragmentData;
    }

    private void ExtractAssets(
        HtmlDocument htmlDoc,
        Uri baseUri,
        FragmentData fragmentData
    )
    {
        var cssLinks = htmlDoc.DocumentNode.SelectNodes(
            "//head/link[@rel='stylesheet' and @href]"
        );
        if (cssLinks != null)
        {
            foreach (var linkNode in cssLinks)
            {
                var href = linkNode.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(href))
                {
                    fragmentData.CssLinks.Add(MakeAbsolute(baseUri, href));
                }
            }
        }

        var scriptNodes = htmlDoc.DocumentNode.SelectNodes(
            "//script[@src]"
        );
        if (scriptNodes != null)
        {
            foreach (var scriptNode in scriptNodes)
            {
                var src = scriptNode.GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrEmpty(src))
                {
                    fragmentData.JsScripts.Add(MakeAbsolute(baseUri, src));
                }
            }
        }
    }

    private void ExtractBody(
        HtmlDocument htmlDoc,
        string? bodyContentSelector,
        FragmentData fragmentData,
        Uri sourceUri
    )
    {
        HtmlNode? contentSourceNode;
        if (!string.IsNullOrWhiteSpace(bodyContentSelector))
        {
            contentSourceNode = htmlDoc.DocumentNode.SelectSingleNode(
                bodyContentSelector
            );
            if (contentSourceNode == null)
            {
                logger.LogWarning(
                    "Body content selector '{BodyContentSelector}' not found in fragment from {SourceUri}. Falling back to body.",
                    bodyContentSelector,
                    sourceUri
                );
                contentSourceNode = htmlDoc.DocumentNode.SelectSingleNode(
                    "//body"
                );
            }
        }
        else
        {
            contentSourceNode = htmlDoc.DocumentNode.SelectSingleNode(
                "//body"
            );
        }
        fragmentData.BodyContent = contentSourceNode?.InnerHtml ??
            string.Empty;
    }

    private string MakeAbsolute(Uri baseUri, string relativeOrAbsoluteUrl)
    {
        if (Uri.TryCreate(
                relativeOrAbsoluteUrl,
                UriKind.Absolute,
                out _
            ))
        {
            return relativeOrAbsoluteUrl;
        }
        try
        {
            return new Uri(baseUri, relativeOrAbsoluteUrl).ToString();
        }
        catch (UriFormatException ex)
        {
            logger.LogWarning(
                ex,
                "Could not make URL absolute. Base: '{BaseUri}', Relative: '{RelativeUrl}'. Returning original.",
                baseUri,
                relativeOrAbsoluteUrl
            );
            return relativeOrAbsoluteUrl;
        }
    }
}
