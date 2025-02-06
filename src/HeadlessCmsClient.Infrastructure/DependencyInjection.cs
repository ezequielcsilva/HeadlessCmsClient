using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Infrastructure.Http;
using HeadlessCmsClient.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;

namespace HeadlessCmsClient.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCmsClient(this IServiceCollection services, string baseUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseUrl);

        services.AddHttpClient<ITokenProvider, TokenProvider>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<ICmsClient, CmsClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHandler>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddTransient<AuthHandler>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}