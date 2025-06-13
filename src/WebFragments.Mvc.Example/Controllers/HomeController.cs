using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebFragments.Mvc.Example.Models;
// If you want to use StaticHtmlFragmentProcessor directly in controller:
// using WebFragments.Core;
// using Microsoft.Extensions.Logging;

namespace WebFragments.Mvc.Example.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    // private readonly StaticHtmlFragmentProcessor _staticProcessor; // For static demo

    public HomeController(ILogger<HomeController> logger /*, StaticHtmlFragmentProcessor staticProcessor */)
    {
        _logger = logger;
        // _staticProcessor = staticProcessor; // For static demo
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Fragments()
    {
        // Construct full URLs for fragments served by this app
        ViewBag.Fragment1Url = $"{Request.Scheme}://{Request.Host}/fragments/fragment1.html";
        ViewBag.Fragment2Url = $"https://car-confessions.pages.dev";
        ViewBag.Fragment3Url = $"https://react-fragment.nbedd2.workers.dev";
        return View();
    }

    // Example for static processing (if needed)
    // public async Task<IActionResult> StaticDemo()
    // {
    //     var targetHtml = "<html><head><title>Static</title></head><body><div id='p1'></div></body></html>";
    //     var defs = new List<StaticHtmlFragmentDefinition> {
    //         new StaticHtmlFragmentDefinition {
    //             FragmentSourceUrl = $"{Request.Scheme}://{Request.Host}/fragments/fragment1.html",
    //             TargetElementId = "p1"
    //         }
    //     };
    //     var result = await _staticProcessor.ProcessHtmlStringAsync(targetHtml, defs);
    //     return Content(result ?? "Error", "text/html");
    // }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
