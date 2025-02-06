using System.Text.Json;
using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using HeadlessCmsClient.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SampleConsoleApp;

public class SampleConsoleApp
{
    public static async Task Main(string[] args)
    {
        var server = WireMockServer.Start();

        server.Given(Request.Create().WithPath("/auth").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBody(JsonSerializer.Serialize(new TokenResponse()
                {
                    BearerToken = "fake-token-123",
                    ExpiryDate = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds()
                })));

        server.Given(Request.Create().WithPath("/documents").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBody(JsonSerializer.Serialize<DocumentMetadata[]>([
                    new DocumentMetadata() { Id = Guid.NewGuid(), Title = "Doc 1", PublishDate = 1712345678, ExpiryDate = 1723456789 },
                    new DocumentMetadata { Id = Guid.NewGuid(), Title = "Doc 2", PublishDate = 1712345678, ExpiryDate = 1723456789 }
                ])));

        Console.WriteLine("Mock server running at: " + server.Urls[0]);

        var serviceCollection = new ServiceCollection();
        var authRequest = new AuthRequest { TenantId = Guid.NewGuid(), Username = "user@example.com", Password = "password" };

        serviceCollection.AddSingleton(authRequest);
        serviceCollection.AddCmsClient(server.Urls[0]);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var cmsClient = serviceProvider.GetRequiredService<ICmsClient>();

        var documents = await cmsClient.GetDocumentsMetadataAsync(CancellationToken.None);
        foreach (var doc in documents)
        {
            Console.WriteLine($"Document: {doc.Title} (ID: {doc.Id})");
        }
    }
}