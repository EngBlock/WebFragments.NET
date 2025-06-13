# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WebFragmentsDotNet is a .NET 9.0 solution for embedding HTML fragments into host applications. It extracts CSS/JS assets from fragments and seamlessly integrates them into server-side rendered pages.

## Solution Structure

- **WebFragments.Core**: Core fragment extraction and processing logic using HtmlAgilityPack
- **WebFragments.AspNetCore.Mvc**: ASP.NET Core MVC integration with Tag Helpers
- **WebFragments.Mvc.Example**: Demo application showing practical usage

## Key Architecture

The solution uses a pipeline approach:
1. `IFragmentExtractor` fetches and parses HTML fragments
2. `FragmentData` holds extracted content and asset URLs
3. `IFragmentAssetCollector` deduplicates CSS/JS assets per request
4. Tag Helpers (`<web-fragment>`, `<web-fragment-styles>`, `<web-fragment-scripts>`) render fragments and manage assets

## Common Commands

Build the solution:
```bash
dotnet build
```

Run the example application:
```bash
dotnet run --project src/WebFragments.Mvc.Example
```

Pack libraries for NuGet:
```bash
dotnet pack src/WebFragments.Core/WebFragments.Core.csproj -c Release
dotnet pack src/WebFragments.AspNetCore.Mvc/WebFragments.AspNetCore.Mvc.csproj -c Release
```

## Development Notes

- All projects target .NET 9.0
- The solution uses project references between libraries
- HttpClient is configured via dependency injection with name "WebFragmentsClient"
- Asset deduplication happens per HTTP request via HttpContext.Items
- XPath selectors are used for fragment content extraction