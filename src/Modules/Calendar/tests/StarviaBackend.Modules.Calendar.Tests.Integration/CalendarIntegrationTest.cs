using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Calendar.Tests.Integration;

/// <summary>
/// All Calendar integration tests share one container (per collection) and run sequentially: the
/// Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(CalendarCollection.Name)]
public abstract class CalendarIntegrationTest : IntegrationTest<CalendarApp>
{
    protected CalendarIntegrationTest(CalendarApp app) : base(app)
    {
        app.Posts.Reset();
    }

    protected HttpClient CreateUserClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);

    protected static async Task<Guid> CreateCalendarAsync(HttpClient client, string name = "My calendar")
    {
        var response = await client.PostAsJsonAsync("/v1/calendars", new { name });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CalendarIdResponse>();
        return body!.ContentCalendarId;
    }

    protected static Task<HttpResponseMessage> DeleteWithBodyAsync(HttpClient client, string url, object body) =>
        client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url) { Content = JsonContent.Create(body) });

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record CalendarIdResponse(Guid ContentCalendarId);

    protected sealed record CalendarResponse(Guid Id, string Name, DateTime CreatedAt);

    protected sealed record CalendarEntryIdResponse(Guid CalendarEntryId);
}

[CollectionDefinition(CalendarCollection.Name)]
public sealed class CalendarCollection : ICollectionFixture<CalendarApp>
{
    public const string Name = "CalendarIntegration";
}
