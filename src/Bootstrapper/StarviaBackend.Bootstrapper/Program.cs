using Serilog;
using StarviaBackend.Bootstrapper.Bootstrap;
using StarviaBackend.Shared.Infrastructure;
using StarviaBackend.Bootstrapper.Bootstrap;

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

app.UseHttpsRedirection();

app.Run();