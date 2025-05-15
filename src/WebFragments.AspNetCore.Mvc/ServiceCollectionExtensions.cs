using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebFragments.Core; // For IFragmentExtractor, FragmentExtractor

namespace WebFragments.AspNetCore.Mvc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebFragments(this IServiceCollection services)
    {
        // Configure a named HttpClient for fetching fragments
        // You can customize Polly policies (retry, circuit breaker) here if needed
        services.AddHttpClient("WebFragmentsClient")
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (compatible; WebFragmentsBot/1.0)"
                );
            });

        services.TryAddScoped<IFragmentExtractor, FragmentExtractor>();
        services.AddHttpContextAccessor();
        services.TryAddScoped<IFragmentAssetCollector, HttpContextFragmentAssetCollector>();
        
        // For StaticHtmlFragmentProcessor if used via DI (optional)
        services.TryAddScoped<StaticHtmlFragmentProcessor>();


        return services;
    }
}