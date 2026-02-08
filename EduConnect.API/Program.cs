using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using EduConnect.Infrastructure.Repositories;
using EduConnect.Infrastructure.Services;
using EduConnect.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
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

// CORS Configuration (origins from appsettings Cors:AllowedOrigins, semicolon-separated)
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string>()?
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

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
