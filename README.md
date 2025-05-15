# WebFragments

**WebFragments** is a .NET 8 solution designed to download HTML content from a URL (a "fragment"), extract its essential assets (CSS, JavaScript) and body content, and then seamlessly integrate this fragment into a host application. It's particularly well-suited for ASP.NET Core MVC applications using Tag Helpers for server-side rendering (SSR) and also provides core logic for static HTML file processing.

This solution helps in building modular web applications where parts of a page (fragments) can be maintained and deployed independently, then composed together on the server.

## Key Features

*   **Dynamic Fragment Embedding:** Fetch HTML content from external or internal URLs at runtime.
*   **Asset Extraction:** Automatically identifies `<link rel="stylesheet">` and `<script src="...">` tags from the fragment.
*   **Asset Management:**
    *   Injects CSS links into the `<head>` of the host page.
    *   Injects JavaScript links at the end of the `<body>` of the host page.
    *   **Deduplicates** assets to prevent loading the same CSS or JS file multiple times if multiple fragments reference them.
*   **Content Injection:**
    *   Replaces a specified element in a host HTML file/string with the fragment's body content (for static processing).
    *   Injects fragment's body content directly into Razor views using Tag Helpers.
*   **Configurable Body Extraction:** Optionally specify an XPath selector to extract only a specific part of the fragment's `<body>`.
*   **ASP.NET Core MVC Integration:** Provides easy-to-use Tag Helpers for embedding fragments and managing their assets within Razor views.
*   **Static HTML Processing:** Includes a processor for scenarios where you need to modify static HTML files or strings directly.
*   **SSR Friendly:** Designed for server-side rendering, ensuring content is available on initial page load for SEO and performance.

## Project Structure

The solution is organized into the following projects:

1.  **`WebFragments.Core`**:
    *   A .NET 8 class library containing the core logic.
    *   `IFragmentExtractor` / `FragmentExtractor`: Services responsible for fetching a fragment URL, parsing its HTML (using HtmlAgilityPack), and extracting `FragmentData` (body content, CSS links, JS script URLs).
    *   `FragmentData`: A model class to hold the extracted information.
    *   `StaticHtmlFragmentProcessor`: A service to take a target HTML (file or string) and inject one or more fragments into it by replacing specified elements and adding assets.

2.  **`WebFragments.AspNetCore.Mvc`**:
    *   A .NET 8 class library providing ASP.NET Core MVC integration.
    *   `WebFragmentTagHelper` (`<web-fragment>`): Fetches a fragment and renders its body content. It also registers the fragment's assets with an asset collector.
    *   `FragmentStylesTagHelper` (`<web-fragment-styles>`): Renders all collected unique CSS `<link>` tags, typically placed in the `<head>`.
    *   `FragmentScriptsTagHelper` (`<web-fragment-scripts>`): Renders all collected unique JS `<script>` tags, typically placed at the end of the `<body>`.
    *   `IFragmentAssetCollector` / `HttpContextFragmentAssetCollector`: Services to collect and deduplicate asset URLs during a request.
    *   `ServiceCollectionExtensions`: Provides `AddWebFragments()` for easy DI setup.

3.  **`WebFragments.Mvc.Example`**:
    *   An ASP.NET Core MVC application demonstrating the usage of the Tag Helpers and core services.
    *   Includes example fragment HTML files served via `wwwroot`.

## Prerequisites

*   .NET 8 SDK
*   For running the `WebFragments.Mvc.Example` project and testing fragment URLs that point to `localhost`: A simple local HTTP server to serve the example fragment files from `wwwroot` if you are testing URLs like `http://localhost:xxxx/fragments/fragment1.html`. The example project itself will serve these files if you use relative paths or construct URLs pointing to its own `wwwroot`.

## Installation & Setup

### For Library Consumers (Using NuGet Packages - Hypothetical)

Once these libraries are packed and published as NuGet packages (e.g., `YourCompany.WebFragments.Core` and `YourCompany.WebFragments.AspNetCore.Mvc`):

1.  **Add NuGet Packages** to your ASP.NET Core MVC project:
    ```bash
    dotnet add package YourCompany.WebFragments.Core
    dotnet add package YourCompany.WebFragments.AspNetCore.Mvc
    ```

2.  **Register Services** in your `Program.cs`:
    ```csharp
    // Program.cs
    using WebFragments.AspNetCore.Mvc; // For AddWebFragments()

    var builder = WebApplication.CreateBuilder(args);

    // Add other services...
    builder.Services.AddControllersWithViews();
    builder.Services.AddWebFragments(); // Registers fragment services and HttpClient

    var app = builder.Build();
    // ... rest of Program.cs
    ```

3.  **Import Tag Helpers** in your `_ViewImports.cshtml`:
    ```cshtml
    @* _ViewImports.cshtml *@
    @using YourProjectName
    @using YourProjectName.Models
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    @addTagHelper *, WebFragments.AspNetCore.Mvc // Add this line
    ```

### For Developers of This Solution

1.  Clone the repository.
2.  Open `WebFragmentsSolution.sln` in Visual Studio or use the .NET CLI.
3.  The `WebFragments.Mvc.Example` project is set up to demonstrate usage.

## Usage

### 1. ASP.NET Core MVC Integration (Tag Helpers)

This is the primary way to use WebFragments in a dynamic web application.

**a. Place Asset Tag Helpers in `_Layout.cshtml`:**

Edit your main layout file (e.g., `Views/Shared/_Layout.cshtml`):

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - My App</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />

    <!-- === Add this for fragment CSS === -->
    <web-fragment-styles />
</head>
<body>
    <header>
        <!-- ... navigation ... -->
    </header>
    <div class="container">
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted">
        <!-- ... footer content ... -->
    </footer>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)

    <!-- === Add this for fragment JavaScript === -->
    <web-fragment-scripts />
</body>
</html>
```

**b. Use the `<web-fragment>` Tag Helper in your Views:**

In any Razor view where you want to embed content:

```cshtml
@{
    ViewData["Title"] = "Page with Fragments";
    // Construct the full URL to your fragment.
    // If served by the same app, you can use Request.Scheme, Request.Host.
    var fragment1Url = $"{Context.Request.Scheme}://{Context.Request.Host}/fragments/my-article-fragment.html";
    var fragment2Url = "https://external.example.com/some-widget.html";
}

<h1>@ViewData["Title"]</h1>

<p>Some introductory content on the main page.</p>

<div class="fragment-container">
    <h3>Article Fragment:</h3>
    <web-fragment source-url="@fragment1Url"></web-fragment>
</div>

<div class="fragment-container" style="margin-top: 20px;">
    <h3>External Widget (extracting specific part):</h3>
    <web-fragment source-url="@fragment2Url"
                  source-body-selector="//div[@id='main-widget-content']">
    </web-fragment>
    <p><em>If the selector above is not found, the entire body of the fragment will be used.</em></p>
</div>

<p>Some content after the fragments.</p>
```

**Attributes for `<web-fragment>`:**

*   `source-url` (required): The absolute URL of the HTML fragment to fetch.
*   `source-body-selector` (optional): An XPath selector to extract a specific part of the fragment's body. If omitted, the entire inner HTML of the fragment's `<body>` tag is used.

**How it Works (ASP.NET Core MVC):**

1.  When the Razor view is rendered, each `<web-fragment>` TagHelper executes.
2.  It calls `IFragmentExtractor` to fetch the `source-url` over HTTP.
3.  The HTML is parsed:
    *   CSS links from `<head>` are collected.
    *   JS script `src` attributes are collected.
    *   The body content (or content matching `source-body-selector`) is extracted.
4.  The extracted body content is rendered in place of the `<web-fragment>` tag.
5.  The collected CSS and JS URLs are passed to `IFragmentAssetCollector`, which stores them (deduplicated) in `HttpContext.Items`.
6.  In `_Layout.cshtml`:
    *   `<web-fragment-styles>` retrieves all unique CSS URLs from the collector and renders them as `<link>` tags.
    *   `<web-fragment-scripts>` retrieves all unique JS URLs and renders them as `<script>` tags.

This ensures that all assets are declared in the correct places in the final HTML document, and only once, even if multiple fragments on the page reference the same asset.

### 2. Static HTML Processing

For scenarios where you need to process a static HTML file or an HTML string (e.g., in a build script, a console application, or a specific backend process):

```csharp
// Example usage in a service or console app
// Ensure you have DI setup for IFragmentExtractor and ILogger,
// or instantiate FragmentExtractor directly if not using DI.

// Assuming you have an instance of StaticHtmlFragmentProcessor
// private readonly StaticHtmlFragmentProcessor _processor;
// public MyService(StaticHtmlFragmentProcessor processor) { _processor = processor; }

public async Task<string> ProcessMyStaticPage()
{
    string targetHtmlFilePath = "path/to/your/target-page.html";
    // OR: string targetHtmlContent = "<html><head>...</head><body><div id='inject1'></div></body></html>";

    var fragmentDefinitions = new List<StaticHtmlFragmentDefinition>
    {
        new StaticHtmlFragmentDefinition
        {
            FragmentSourceUrl = "http://localhost:8000/fragments/fragment1.html",
            TargetElementId = "placeholder-for-fragment1",
            SourceBodySelector = "//div[@class='main-content']" // Optional
        },
        new StaticHtmlFragmentDefinition
        {
            FragmentSourceUrl = "http://localhost:8000/fragments/fragment2.html",
            TargetElementId = "placeholder-for-fragment2"
        }
    };

    string? modifiedHtml = await _processor.ProcessHtmlFileAsync(targetHtmlFilePath, fragmentDefinitions);
    // OR: string? modifiedHtml = await _processor.ProcessHtmlStringAsync(targetHtmlContent, fragmentDefinitions);

    if (modifiedHtml != null)
    {
        // Save modifiedHtml to a new file or use it as needed
        Console.WriteLine("HTML processed successfully.");
        return modifiedHtml;
    }
    else
    {
        Console.WriteLine("HTML processing failed.");
        return string.Empty; // Or handle error
    }
}
```

**How it Works (Static Processing):**

1.  `StaticHtmlFragmentProcessor` loads the target HTML (from file or string).
2.  For each `StaticHtmlFragmentDefinition`:
    *   It uses `IFragmentExtractor` to fetch and parse the fragment from `FragmentSourceUrl`.
    *   The fragment's CSS links are added to the target HTML's `<head>` (deduplicated).
    *   The HTML element in the target document with the ID matching `TargetElementId` is located.
    *   The content of this target element is replaced with the extracted body content of the fragment.
    *   The fragment's JS scripts are added to the end of the target HTML's `<body>` (deduplicated).
3.  The modified HTML string is returned.

## Configuration

*   **HttpClient ("WebFragmentsClient"):** The `AddWebFragments()` extension method registers a named `HttpClient` ("WebFragmentsClient"). You can further configure this client (e.g., add Polly policies for retry/circuit breaker, default headers) in `Program.cs` if needed:
    ```csharp
    builder.Services.AddHttpClient("WebFragmentsClient")
        .ConfigureHttpClient(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyCustomApp/1.0 WebFragmentsBot/1.0");
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        // .AddTransientHttpErrorPolicy(policyBuilder => ...) // Example: Add Polly
        ;
    ```

## SSR (Server-Side Rendering) Compatibility

This library is designed with SSR in mind.
*   **ASP.NET Core MVC/Razor Pages:** The Tag Helpers execute server-side, embedding the fragment content and asset links directly into the HTML response sent to the browser. This is ideal for SEO and initial page load performance.
*   **JavaScript Frameworks with SSR (e.g., Next.js, Nuxt.js, Angular Universal):**
    *   If ASP.NET Core serves the initial shell that a JS framework hydrates, the fragments will be part of that initial server-rendered HTML.
    *   Alternatively, the `WebFragments.Core` library can be used by a backend (even a Node.js SSR backend) by calling an API endpoint (potentially hosted by an ASP.NET Core app) that uses `IFragmentExtractor` to get fragment data (HTML, CSS, JS links) as JSON. The JS SSR process would then integrate this data into its own rendered output.

## Building the NuGet Packages

To create `.nupkg` files for `WebFragments.Core` and `WebFragments.AspNetCore.Mvc`:

1.  Ensure the `<GeneratePackageOnBuild>`, `<PackageId>`, `<Version>`, etc., properties are set correctly in the `.csproj` files of these libraries.
2.  From the solution root directory (`WebFragmentsSolution`):
    ```bash
    dotnet pack src/WebFragments.Core/WebFragments.Core.csproj -c Release
    dotnet pack src/WebFragments.AspNetCore.Mvc/WebFragments.AspNetCore.Mvc.csproj -c Release
    ```
    The packages will be created in their respective `bin/Release` folders.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the [MIT License](LICENSE.txt) (assuming you add one).
