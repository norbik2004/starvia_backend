using dotenv.net;
using Google.GenAI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Web.Helpers;
using Web.Middlewares;
using Core;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Core.Application.Services;
using Core.Application.Services.Gemini;
using Repository;
using Repository.Repositories;
using Repository.Seeds;
using Service;
using Service.Email;
using Service.Gemini;
using Service.LinkedIn;
using Service.Mapping;
using Service.Services;
using Web.Helpers;
using Core.Application;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

ProgramHelpers.AddSingletons(builder);

builder.Services.AddHttpClient<ILinkedInService, LinkedInService>();

var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;

if(redisConnectionString is null)
{
    throw new InvalidOperationException("Redis connection string is not configured.");
}

var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddDbContext<TrDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
            );
        }));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.SignIn.RequireConfirmedEmail = true;
})
    .AddEntityFrameworkStores<TrDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = 429,
            Description = "Too many confirmation email requests. Please wait before trying again.",
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };

    options.AddPolicy("email-confirm", httpContext =>
    {
        var email = httpContext.Request.Query["email"]
            .ToString()
            ?.ToLowerInvariant()
            ?? "unknown";

        var ip = httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown-ip";

        var partitionKey = $"{email}:{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(20),
                QueueLimit = 0
            });
    });
});

// to do modyfikacji pozniej
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});


builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = "ContentForge_Cookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };

});

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromDays(3);
    options.Name = "ContentForgeTokenProvider";
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSettingRepository, UserSettingRepository>();
builder.Services.AddScoped<IUserSettingService, UserSettingService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IUserPlatformRepository, UserPlatformRepository>();
builder.Services.AddScoped<IUserPlatformService, UserPlatformService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPostPublicationRepository, PostPublicationRepository>();
builder.Services.AddScoped<IPostPublishService, PostPublicationService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IUserPromptRepository, UserPromptRepository>();
builder.Services.AddScoped<IUserPromptService, UserPromptService>();
builder.Services.AddScoped<IGeminiModelHealthService, GeminiModelHealthService>();
builder.Services.AddScoped<IUserUploadedFileRepository, UserUploadedFileRepository>();
builder.Services.AddScoped<IUserUploadedFileService, UserUploadedFileService>();
builder.Services.AddScoped<UserUploadedFilePreviewUrlResolver>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
*/

app.UseSwagger();
app.UseSwaggerUI();

await DbMigrate.MigrateDatabase(app);
using (var scope = app.Services.CreateScope())
{
    await RoleSeed.Seed(scope.ServiceProvider);
    await SeedUsers.Seed(scope.ServiceProvider);
    await SeedPlatforms.Seed(scope.ServiceProvider);
}

app.UseRouting();

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

await app.RunAsync();
