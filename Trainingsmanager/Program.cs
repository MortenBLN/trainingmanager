using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Trainingsmanager.Database;
using Trainingsmanager.Helper;
using Trainingsmanager.Mappers;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;
using Trainingsmanager.Services;
using Trainingsmanager.Services.SchedulerServices;

var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints();
bld.Services.SwaggerDocument(config =>
{
    config.DocumentSettings = s =>
    {
        s.Title = "Trainingsession API";
        s.Version = "v1";
    };
});

// For frontend access
bld.Services.AddCors();

// Authentication

bld.Services.AddAuthenticationJwtBearer(key => key.SigningKey = bld.Configuration["JwtSecret"]);
bld.Services.AddAuthorization();

// Database
bld.Services.AddDbContextFactory<Context>(options =>
{
    //options.UseNpgsql("Host=ep-black-meadow-a9ynyveo-pooler.gwc.azure.neon.tech;Port=5432;Username=neondb_owner;Password=npg_eSu1Kg2mtoPR;Database=mystampDB;SSL Mode=Require;Trust Server Certificate=true;");
    options.UseNpgsql("Host=ep-black-meadow-a9ynyveo-pooler.gwc.azure.neon.tech;Database=trainigmanager;Username=neondb_owner;Password=npg_eSu1Kg2mtoPR;SSL Mode=Require;Trust Server Certificate=true");
    //options.UseNpgsql("Host=localhost;Port=5432;Database=trainingsessions;Username=postgres;Password=apfelringe4");
});

// Services
bld.Services.AddScoped<IUserService, UserService>();
bld.Services.AddScoped<ISessionService, SessionService>();
bld.Services.AddScoped<ISubscriptionService, SubscriptionService>();
bld.Services.AddScoped<IAuthService, AuthService>();
bld.Services.AddScoped<ISessionGroupService, SessionGroupService>();

// Scheduler-Services
bld.Services.AddScoped<SessionCleanupService>();
bld.Services.AddHostedService<SessionCleanupHostedService>();

// Repositories
bld.Services.AddScoped<ISessionRepository, SessionRepository>();
bld.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
bld.Services.AddScoped<ISessionGroupRepository, SessionGroupRepository>();
bld.Services.AddScoped<IAuthRepository, AuthRepository>();

// Mappers
bld.Services.AddScoped<ISessionMapper, SessionMapper>();
bld.Services.AddScoped<IAuthMapper, AuthMapper>();

// Helpers
bld.Services.AddScoped<ISessionHelper,  SessonHelper>();

// Options
bld.Services.Configure<FixedSubsOptions>(bld.Configuration);
bld.Services.Configure<JwtTokenOptions>(bld.Configuration);

// Application Insights
using var channel = new InMemoryChannel();
bld.Services.Configure<TelemetryConfiguration>(config => config.TelemetryChannel = channel);

// Load from appsettings.json or Azure environment variables
var connectionString = bld.Configuration.GetConnectionString("APPLICATIONINSIGHTS_CONNECTION_STRING");

// Add Application Insights telemetry + logger
//bld.Services.Insi(options =>
//{
//    options.ConnectionString = connectionString;
//});

bld.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: config =>
        config.ConnectionString = connectionString,
    configureApplicationInsightsLoggerOptions: options => { }
);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // service registrations
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("App starting...");

logger.LogInformation("Logger is working...");

var app = bld.Build();

// For frontend access
app.UseCors(x => x.AllowAnyOrigin());

// For wwwroot folder
app.UseStaticFiles();

app.UseFastEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerGen();
app.Run();

// needed for Tests
public partial class Program { }
