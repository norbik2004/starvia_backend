using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace StarviaBackend.Modules.Ai.Tests.Integration.Chats;

public sealed class ChatsSwaggerTests(AiApp app) : AiIntegrationTest(app)
{
    [Fact]
    public async Task Swagger_document_lists_every_chat_endpoint()
    {
        // Act
        var response = await App.CreateClient().GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        var operations = paths.EnumerateObject()
            .Where(path => path.Name.StartsWith("/v1/ai/chats", StringComparison.Ordinal))
            .SelectMany(path => path.Value.EnumerateObject().Select(method => $"{method.Name.ToUpperInvariant()} {path.Name}"))
            .ToList();

        operations.Should().BeEquivalentTo(
        [
            "POST /v1/ai/chats",
            "GET /v1/ai/chats",
            "GET /v1/ai/chats/{chatId}",
            "PUT /v1/ai/chats/{chatId}",
            "DELETE /v1/ai/chats/{chatId}",
            "POST /v1/ai/chats/{chatId}/messages",
        ]);
    }
}
