using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Infrastructure.Repositories;
using EduConnect.Infrastructure.Services;
using EduConnect.API.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (console + file; do not log credentials — use LoggerExtensions which redact)
var logPath = builder.Configuration["Serilog:File:Path"] ?? throw new InvalidOperationException("Serilog:File:Path is required in config.");
var logRetainedDays = builder.Configuration.GetValue<int?>("Serilog:File:RetainedFileCountDays");
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        logPath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: logRetainedDays,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} (Method={Method}, LineNumber={LineNumber}){NewLine}{Exception}",
        shared: true);
Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// FluentValidation (server-side request validation)
var validatorAssembly = typeof(EduConnect.Application.Validators.LoginRequestValidator).Assembly;
builder.Services.AddValidatorsFromAssembly(validatorAssembly);
#pragma warning disable CS0618
builder.Services.AddScoped<IValidatorFactory, ServiceProviderValidatorFactory>();
#pragma warning restore CS0618

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<EduConnect.API.Filters.ValidationFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new EduConnect.API.Json.UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new EduConnect.API.Json.NullableUtcDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "EduConnect API", 
        Version = "v1",
        Description = "EduConnect Freelance School - Agency Model API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// CORS (required in appsettings Cors:AllowedOrigins, semicolon-separated)
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string>();
var allowedOrigins = string.IsNullOrWhiteSpace(corsOrigins)
    ? throw new InvalidOperationException("Cors:AllowedOrigins is required in appsettings.")
    : corsOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Application Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DapperContext>(sp => 
    new DapperContext(connectionString ?? throw new InvalidOperationException("Connection string not found")));
builder.Services.AddApplicationServices();

// Encryption Service
var encryptionKey = builder.Configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
builder.Services.AddSingleton<IEncryptionService>(sp => new EncryptionService(encryptionKey));

// Rate limiting (from appsettings RateLimiting)
var rateLimit = builder.Configuration.GetSection("RateLimiting");
var globalPermit = rateLimit.GetValue("GlobalPermitLimit", 100);
var globalWindowMin = rateLimit.GetValue("GlobalWindowMinutes", 1);
var authPermit = rateLimit.GetValue("AuthPermitLimit", 10);
var authWindowMin = rateLimit.GetValue("AuthWindowMinutes", 1);
var rejectedMsg = rateLimit["RejectedMessage"] ?? "Too many requests. Try again later.";
var rejectedCode = rateLimit["RejectedCode"] ?? "RATE_LIMITED";
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = globalPermit, Window = TimeSpan.FromMinutes(globalWindowMin) }));
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = authPermit, Window = TimeSpan.FromMinutes(authWindowMin) }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = rejectedMsg, code = rejectedCode }, token);
    };
});

// Health checks (liveness = process; readiness = DB)
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("OK"), tags: new[] { "live" })
    .AddDbContextCheck<ApplicationDbContext>("db", tags: new[] { "ready" });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Swagger disabled - remove the comments below to re-enable
    // app.UseSwagger();
    // app.UseSwaggerUI();
    // Disable HTTPS redirection in development to avoid issues with local HTTP connections
    // app.UseHttpsRedirection();
}
else
{
    // Only use HTTPS redirection in production
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngularApp");
app.UseCustomMiddleware();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
var healthLive = builder.Configuration["HealthChecks:LivePath"] ?? "/health/live";
var healthReady = builder.Configuration["HealthChecks:ReadyPath"] ?? "/health/ready";
app.MapHealthChecks(healthLive, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("live") });
app.MapHealthChecks(healthReady, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

// Timezone for "today" and reports (from appsettings TimeZone)
EduConnect.Infrastructure.MyanmarTimeHelper.Initialize(builder.Configuration);

// Ensure database is created and seed data (SeedData from appsettings)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    await context.Database.EnsureCreatedAsync();
    await EduConnect.Infrastructure.Data.SchemaEnsurer.EnsureMissingTablesAsync(context, cancellationToken: default);
    await EduConnect.Infrastructure.Data.DbSeeder.SeedAsync(context, userManager, roleManager, configuration);
}

app.Run();
