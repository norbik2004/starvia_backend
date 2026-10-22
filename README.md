# ModularMonolithBoilerplate

A .NET 10 modular-monolith starter: layered modules, CQRS with custom dispatchers, ASP.NET
Identity + JWT, MassTransit (in-memory) messaging, EF Core / PostgreSQL, and a full integration
+ architecture test setup. Modelled on the `Pulsar` architecture and the `RescueGlass`
integration-test approach.

## Solution layout

```
src/
  Bootstrapper/ModularMonolith.Bootstrapper/        # the single runnable host (Program.cs + Bootstrap/*)
  Shared/
    ModularMonolith.Shared.Abstractions/            # contracts only (CQRS, domain base, auth, context, messaging)
    ModularMonolith.Shared.Infrastructure/          # implementations + composition root (AddInfrastructure/UseInfrastructure)
    ModularMonolith.Shared.Tests.Integration/       # shared test harness (WebApplicationFactory + Testcontainers + fake auth)
  Modules/
    Accounts/
      ...Accounts.Core/                             # domain: entities, repository interfaces, exceptions
      ...Accounts.Application/                       # CQRS: commands/queries/validators + integration event + consumer
      ...Accounts.Infrastructure/                    # EF: write/read DbContexts, configs, migrations, query handlers
      ...Accounts.Api/                               # Ardalis endpoints + AccountsModule registration
      Integrations/
        ...Accounts.Integrations.Sample/            # EXAMPLE "clean integration" — not wired anywhere (see below)
      tests/
        ...Accounts.Tests.Integration/              # integration + messaging tests for the module
tests/
  ModularMonolith.Tests.Architecture/               # NetArchTest rules enforcing the layering
```

Layer dependencies: `Api → Infrastructure → Application → Core`; `Application`/`Infrastructure → Shared.*`.
The Bootstrapper references only each module's `Api` (+ `Infrastructure` for EF tooling) and `Shared.Infrastructure`.

### Feature-first layout inside a module

Each layer is organized **by feature** (here the feature is `Users`), then by concern:

```
Accounts.Application/Users/
  Commands/{Action}/{Action}Command.cs + {Action}Handler.cs + {Action}Validator.cs
  Queries/{Action}/{Action}Query.cs                       # + DTO
  Events/{Action}/{Action}Event.cs + {Action}Consumer.cs

Accounts.Infrastructure/EF/
  Contexts/AccountsWriteDbContext.cs, AccountsReadDbContext.cs
  Users/
    Configurations/Write/{Entity}Configuration.cs
    Configurations/Read/{ReadModel}Configuration.cs
    Configurations/Read/Models/{ReadModel}.cs
    Repositories/{Aggregate}Repository.cs
    Queries/{Action}Handler.cs                             # query handlers (read side)
  Initializers/AccountsSeeder.cs, AccountsDataInitializer.cs
  Migrations/

Accounts.Api/Endpoints/Users/
  UsersEndpoint.cs                                          # route/tag/role constants
  Commands/{Action}/{Action}Endpoint.cs
  Queries/{Action}/{Action}Endpoint.cs
```

The domain entities live in `Accounts.Core/Users/Entities` and include the full Identity model
(`User`, `Role`, `UserRole`, `UserClaim`, `RoleClaim`, `UserLogin`, `UserToken`) with read-only
navigations for the query side. Command handlers live in `Application`; query handlers live in
`Infrastructure` (they need the read `DbContext`).

## Prerequisites

- .NET SDK 10 (`global.json` pins the band)
- Docker (only for the integration tests — Testcontainers spins up PostgreSQL)

## Running

```bash
# 1. A local Postgres (or point appsettings.Development.json at your own)
docker run --name mm-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=modular_monolith -p 5432:5432 -d postgres:16-alpine

# 2. Run the host (applies migrations + seeds roles on startup)
dotnet run --project src/Bootstrapper/ModularMonolith.Bootstrapper
# Swagger UI: http://localhost:5080/swagger
```

Endpoints: `POST /v1/accounts/register`, `POST /v1/accounts/sign-in` (returns a JWT),
`GET /v1/accounts/me` (auth), `GET /v1/accounts` (admin), `GET /v1/health/{live,ready}`.

## Tests

```bash
dotnet test tests/ModularMonolith.Tests.Architecture                                   # fast, no Docker
dotnet test src/Modules/Accounts/tests/ModularMonolith.Modules.Accounts.Tests.Integration   # needs Docker
```

The integration harness (`BoilerplateApp`) boots the real host against a throwaway Testcontainers
Postgres, swaps JWT for a header-driven `FakeAuthHandler`, and strips hosted services so nothing
races the fresh schema. `AccountsApp` applies the module's migrations, seeds roles, and starts the
bus. Isolation is by **unique data per test** (no shared-DB reset). If you later want parallel,
clean-state tests, add [Respawn](https://github.com/jbogard/Respawn) to reset the DB between tests
and drop the `[Collection]` serialization.

## Conventions

- **CQRS**: commands/queries are public records implementing `ICommand<T>` / `IQuery<T>`; handlers are
  `internal` and end with `Handler`. Endpoints inject `IDispatcher`, which routes to the handler and
  runs any FluentValidation `IValidator<T>` first.
- **Encapsulation**: entities, handlers, DbContexts and endpoints are `internal`; a module's public
  surface is its commands/queries/DTOs/events and its `XyzModule` registration class.
- **Read/write split**: writes go through the Identity `AccountsWriteDbContext`; queries read from
  the no-tracking `AccountsReadDbContext`.
- **Auditing**: implement `IAuditable` (or derive `AuditableEntity`) and the EF interceptor fills the
  audit columns automatically.

## Messaging (MassTransit)

MassTransit runs with the **in-memory transport** (`AddMessaging` in `Shared.Infrastructure`). The
Accounts module shows the full loop: `RegisterUserHandler` publishes `UserRegisteredEvent`, and
`UserRegisteredConsumer` handles it. Each module contributes consumers via a `RegisterXConsumers`
hook wired in `Bootstrap/Messaging.cs`.

To use a real broker later, swap `UsingInMemory(...)` for `UsingRabbitMq`/`UsingAzureServiceBus`/etc.
in `Shared.Infrastructure/Messaging/Extensions.cs` — modules don't change.

See `UserRegisteredMessagingTests` for the recommended way to test a consumer in isolation with
MassTransit's in-memory `ITestHarness`.

## Adding a new module

1. Create `Core`, `Application`, `Infrastructure`, `Api` projects under `src/Modules/<Name>/`,
   following the Accounts naming and layer references. Add `InternalsVisibleTo` for the inner layers +
   the module's test project.
2. `Application.AddApplication()` and `Infrastructure.AddInfrastructure()` each call
   `services.RegisterHandlers(Assembly.GetExecutingAssembly())` so their handlers/validators are found.
3. Register DbContexts with `services.AddPostgres<TContext>()`.
4. Expose `<Name>Module.Register<Name>Module()` (+ `Register<Name>Consumers()` if it has consumers)
   in the `Api` project.
5. Wire it in `Bootstrap/Modules.cs` (and `Bootstrap/Messaging.cs`). Generate the EF migration:
   ```bash
   ASPNETCORE_ENVIRONMENT=Development dotnet ef migrations add Initial<Name> \
     --project src/Modules/<Name>/...Infrastructure \
     --startup-project src/Bootstrapper/ModularMonolith.Bootstrapper \
     --context <Name>WriteDbContext --output-dir EF/Migrations
   ```
6. Add a test project deriving `BoilerplateApp` (see `AccountsApp`) and copy the architecture tests
   for the new namespaces.

## Integrations (external systems) — the pattern

`ModularMonolith.Modules.Accounts.Integrations.Sample` is a **reference example** demonstrating the
anti-corruption-layer pattern for talking to an external system. **It is intentionally not
referenced or wired anywhere** — it exists so you can copy it.

The pattern:

- **Port** (`ISampleClient`): the interface the module depends on, expressed in domain-shaped terms
  (`SampleProfile`). The module never sees the vendor's DTOs.
- **Vendor DTOs** (`External/SampleApiResponse`): the wire format, kept `internal` to the integration
  project so it can't leak.
- **Adapter** (`SampleClient`): a typed `HttpClient` that calls the vendor, maps its response into the
  domain shape, and translates transport failures into a `BusinessException`
  (`SampleIntegrationException`) that the error middleware maps to a clean HTTP response.
- **Options** (`SampleOptions`): bound from configuration (`integrations:sample`).
- **Registration** (`AddSampleIntegration`): registers the typed client.

To actually use an integration in a module:

1. Add a project reference from the module's `Infrastructure` to the integration project.
2. Call `services.AddSampleIntegration(configuration)` from the module's `AddInfrastructure`.
3. Inject the port (`ISampleClient`) into a handler.
4. Add the `integrations:sample` config section (BaseUrl / ApiKey / TimeoutSeconds).

Put each external system in its own `Integrations/<Vendor>` project so vendor SDKs and DTOs never
bleed into the module.

## Central package management

All package versions live in `Directory.Packages.props`; shared build settings (TFM, nullable,
implicit usings) in `Directory.Build.props`.

### Third-party licensing note

- **MassTransit** is pinned to `8.x` (Apache-2.0). MassTransit `9.x` moved to a commercial license —
  upgrade only if you have one.
- **FluentAssertions** `8.x` requires a paid license for commercial use. If that's a concern, pin to
  `7.x` or switch the test assertions to a permissively-licensed alternative.
```
