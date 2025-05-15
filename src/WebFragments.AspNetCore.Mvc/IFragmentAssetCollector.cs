namespace WebFragments.AspNetCore.Mvc;

public interface IFragmentAssetCollector
{
    void AddCssLink(string cssUrl);
    void AddJsScript(string jsUrl);
    IEnumerable<string> GetCssLinks();
    IEnumerable<string> GetJsScripts();
}