using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace WebFragments.Core;

public class StaticHtmlFragmentDefinition
{
    public required string FragmentSourceUrl { get; set; }
    public required string TargetElementId { get; set; }
    public string? SourceBodySelector { get; set; }
}

public class StaticHtmlFragmentProcessor
{
    private readonly IFragmentExtractor _fragmentExtractor;
    private readonly ILogger<StaticHtmlFragmentProcessor> _logger;

    public StaticHtmlFragmentProcessor(
        IFragmentExtractor fragmentExtractor,
        ILogger<StaticHtmlFragmentProcessor> logger
    )
    {
        _fragmentExtractor = fragmentExtractor;
        _logger = logger;
    }

    public async Task<string?> ProcessHtmlFileAsync(
        string targetHtmlFilePath,
        IEnumerable<StaticHtmlFragmentDefinition> fragmentDefinitions
    )
    {
        if (!File.Exists(targetHtmlFilePath))
        {
            _logger.LogError(
                "Target HTML file not found: {TargetHtmlFilePath}",
                targetHtmlFilePath
            );
            return null;
        }
        var targetHtmlContent = await File.ReadAllTextAsync(
            targetHtmlFilePath
        );
        return await ProcessHtmlStringAsync(
            targetHtmlContent,
            fragmentDefinitions
        );
    }

    public async Task<string?> ProcessHtmlStringAsync(
        string targetHtmlContent,
        IEnumerable<StaticHtmlFragmentDefinition> fragmentDefinitions
    )
    {
        var targetDoc = new HtmlDocument();
        targetDoc.LoadHtml(targetHtmlContent);

        var headNode = GetOrCreateHeadNode(targetDoc);
        var bodyNode = targetDoc.DocumentNode.SelectSingleNode("//body");

        if (bodyNode == null)
        {
            _logger.LogError(
                "Target HTML does not have a <body> element. Cannot proceed."
            );
            return targetHtmlContent;
        }

        var allAddedCssHrefs = GetExistingAssets(headNode, ".//link[@rel='stylesheet' and @href]", "href");
        var allAddedJsSrcs = GetExistingAssets(bodyNode, ".//script[@src]", "src");

        foreach (var def in fragmentDefinitions)
        {
            _logger.LogInformation(
                "Processing fragment from '{FragmentSourceUrl}' for target ID '{TargetElementId}'",
                def.FragmentSourceUrl,
                def.TargetElementId
            );
            var fragmentData =
                await _fragmentExtractor.ExtractFragmentDataAsync(
                    def.FragmentSourceUrl,
                    def.SourceBodySelector
                );

            if (fragmentData == null)
            {
                _logger.LogWarning(
                    "Failed to extract data for fragment: {FragmentSourceUrl}",
                    def.FragmentSourceUrl
                );
                continue;
            }

            InjectAssets(targetDoc, headNode, fragmentData.CssLinks, allAddedCssHrefs, "link", "stylesheet", "href");
            ReplaceElementContent(targetDoc, def.TargetElementId, fragmentData.BodyContent);
            InjectAssets(targetDoc, bodyNode, fragmentData.JsScripts, allAddedJsSrcs, "script", null, "src", appendToBodyEnd: true);
        }
        return targetDoc.DocumentNode.OuterHtml;
    }

    private HtmlNode GetOrCreateHeadNode(HtmlDocument doc)
    {
        var headNode = doc.DocumentNode.SelectSingleNode("//head");
        if (headNode == null)
        {
            _logger.LogDebug("No <head> element found, creating one.");
            headNode = doc.CreateElement("head");
            var htmlNode = doc.DocumentNode.SelectSingleNode("/html") ?? doc.DocumentNode;
            htmlNode.PrependChild(headNode);
        }
        return headNode;
    }

    private HashSet<string> GetExistingAssets(HtmlNode parentNode, string xpath, string attributeName)
    {
        return parentNode
            .SelectNodes(xpath)
            ?.Select(n => n.GetAttributeValue(attributeName, ""))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private void InjectAssets(HtmlDocument doc, HtmlNode parentNode, List<string> assetUrls, HashSet<string> existingAssets, string tagName, string? relValue, string srcOrHrefAttribute, bool appendToBodyEnd = false)
    {
        foreach (var assetUrl in assetUrls)
        {
            if (existingAssets.Add(assetUrl)) // True if added (not a duplicate)
            {
                var assetNode = doc.CreateElement(tagName);
                if (!string.IsNullOrEmpty(relValue))
                {
                    assetNode.SetAttributeValue("rel", relValue);
                }
                assetNode.SetAttributeValue(srcOrHrefAttribute, assetUrl);
                if (appendToBodyEnd && parentNode.Name.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    parentNode.AppendChild(assetNode);
                }
                else
                {
                    parentNode.PrependChild(assetNode); // Prepend in head for CSS
                }
                _logger.LogDebug("Added asset to {ParentNodeName}: {AssetUrl}", parentNode.Name, assetUrl);
            }
        }
    }

    private void ReplaceElementContent(HtmlDocument doc, string elementId, string newHtmlContent)
    {
        var injectionElement = doc.GetElementbyId(elementId);
        if (injectionElement == null)
        {
            _logger.LogWarning("Target element with ID '{ElementId}' not found.", elementId);
            return;
        }

        var parent = injectionElement.ParentNode;
        if (parent != null)
        {
            var tempDoc = new HtmlDocument();
            tempDoc.LoadHtml($"<div>{newHtmlContent}</div>"); // Wrap to handle multiple root elements
            var newNodes = tempDoc.DocumentNode.SelectSingleNode("//div")?.ChildNodes.ToList();

            foreach (var newNode in newNodes ?? [])
            {
                parent.InsertBefore(newNode, injectionElement);
            }
            injectionElement.Remove();
            _logger.LogDebug("Replaced content of element ID '{ElementId}'.", elementId);
        }
        else
        {
            _logger.LogWarning("Target element '{ElementId}' has no parent, cannot replace.", elementId);
        }
    }
}
