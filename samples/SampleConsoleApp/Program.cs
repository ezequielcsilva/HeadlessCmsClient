using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using HeadlessCmsClient.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace SampleConsoleApp;

public class SampleConsoleApp
{
    public static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        var authRequest = new AuthRequest { TenantId = Guid.NewGuid(), Username = "user@example.com", Password = "password" };

        serviceCollection.AddSingleton(authRequest);
        serviceCollection.AddCmsClient("https://api.example.com");
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var cmsClient = serviceProvider.GetRequiredService<ICmsClient>();

        var documents = await cmsClient.GetDocumentsMetadataAsync(CancellationToken.None);
        foreach (var doc in documents)
        {
            Console.WriteLine($"Document: {doc.Title} (ID: {doc.Id})");
        }
    }
}