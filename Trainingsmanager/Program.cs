using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Trainingsmanager.Database;
using Trainingsmanager.Helper;
using Trainingsmanager.Mappers;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;
using Trainingsmanager.Services;
using Trainingsmanager.Services.EmailServices;
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

var connectionString = bld.Configuration.GetConnectionString("DefaultConnection");

// Databasestring is overriden in azure
bld.Services.AddDbContextFactory<Context>(options =>
{
    options.UseNpgsql(connectionString);
});

// Services
bld.Services.AddScoped<IUserService, UserService>();
bld.Services.AddScoped<ISessionService, SessionService>();
bld.Services.AddScoped<ISubscriptionService, SubscriptionService>();
bld.Services.AddScoped<IAuthService, AuthService>();
bld.Services.AddScoped<ISessionGroupService, SessionGroupService>();
bld.Services.AddScoped<IEmailService, EmailService>();
bld.Services.AddScoped<ISessionTemplateService, SessionTemplateService>();

// Scheduler-Services
bld.Services.AddScoped<SessionCleanupService>();
bld.Services.AddHostedService<SessionCleanupHostedService>();

// Repositories
bld.Services.AddScoped<ISessionRepository, SessionRepository>();
bld.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
bld.Services.AddScoped<ISessionGroupRepository, SessionGroupRepository>();
bld.Services.AddScoped<IAuthRepository, AuthRepository>();
bld.Services.AddScoped<ISessionTemplateRepository, SessionTemplateRepository>();

// Mappers
bld.Services.AddScoped<ISessionMapper, SessionMapper>();
bld.Services.AddScoped<IAuthMapper, AuthMapper>();
bld.Services.AddScoped<ISubscriptionMapper, SubscriptionMapper>();
bld.Services.AddScoped<ISessionTemplateMapper, SessionTemplateMapper>();

// Helpers
bld.Services.AddScoped<ISessionHelper,  SessonHelper>();

// Options
bld.Services.Configure<FixedSubsOptions>(bld.Configuration);
bld.Services.Configure<JwtTokenOptions>(bld.Configuration);
bld.Services.Configure<EMailOptions>(bld.Configuration.GetSection("EmailSettings"));

var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

// Ensure the directory exists
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Replace built-in logging with Serilog
bld.Host.UseSerilog();

var app = bld.Build();

// Auto applies DB Updates
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<Context>>();
    var db = await dbFactory.CreateDbContextAsync();

    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        Log.Information("Applying {Count} pending EF Core migrations...", pendingMigrations.Count());
        foreach (var migration in pendingMigrations)
        {
            Log.Information(" - {Migration}", migration);
        }

        await db.Database.MigrateAsync();
        Log.Information("Migrations applied successfully.");
    }
    else
    {
        Log.Information("No pending EF Core migrations. Database is up to date.");
    }
}

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
