using ModularMonolith.Bootstrapper.Bootstrap;
using Serilog;
using StarviaBackend.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

// 1. Modules register their own services...
builder.Services.AddModules(builder.Configuration);
// 2. ...then their bus consumers...
builder.Services.AddAppMessaging(builder.Configuration);
// 3. ...then the shared cross-cutting plumbing.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseInfrastructure();
app.MapAppEndpoints();
app.MapControllers();

await app.RunAsync();

// Exposed so WebApplicationFactory<Program> can boot the host in integration tests.
namespace ModularMonolith.Bootstrapper
{
    public partial class Program;
}
