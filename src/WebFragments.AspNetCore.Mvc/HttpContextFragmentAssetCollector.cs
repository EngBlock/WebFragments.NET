using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebFragments.AspNetCore.Mvc;

public class HttpContextFragmentAssetCollector(IHttpContextAccessor httpContextAccessor) : IFragmentAssetCollector
{
    private const string CssKey = "WebFragments_CssLinks_v1";
    private const string JsKey = "WebFragments_JsScripts_v1";

    private HttpContext HttpContext => httpContextAccessor.HttpContext ??
                                       throw new InvalidOperationException("HttpContext is not available.");

    private HashSet<string> GetOrCreateSet(string key)
    {
        if (HttpContext.Items[key] is not HashSet<string> set)
        {
            set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HttpContext.Items[key] = set;
        }
        return set;
    }

    public void AddCssLink(string cssUrl)
    {
        if (!string.IsNullOrWhiteSpace(cssUrl)) GetOrCreateSet(CssKey).Add(cssUrl);
    }

    public void AddJsScript(string jsUrl)
    {
        if (!string.IsNullOrWhiteSpace(jsUrl)) GetOrCreateSet(JsKey).Add(jsUrl);
    }

    public IEnumerable<string> GetCssLinks() => GetOrCreateSet(CssKey).ToList();
    public IEnumerable<string> GetJsScripts() => GetOrCreateSet(JsKey).ToList();
}