using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Bakery.Api.Authentication;
using Bakery.Api.Middleware;
using Bakery.Api.Seeding;
using Bakery.Api.Swagger;
using Bakery.Application;
using Bakery.Application.Authentication;
using Bakery.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<ProductRequestExampleSchemaFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter a JWT access token using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    options.IncludeXmlComments(
        Path.Combine(AppContext.BaseDirectory, "Bakery.Api.xml"),
        includeControllerXmlComments: true);
});
builder.Services.AddHealthChecks();
builder.Services.AddApplication();

var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "JWT signing key is required. Configure Jwt:SigningKey or JWT__SIGNINGKEY.");
    }

    signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    Console.Error.WriteLine(
        "WARNING: Using a temporary development-only JWT signing key. Existing tokens will be invalid after restart.");
}

if (Encoding.UTF8.GetByteCount(signingKey) < 32)
{
    throw new InvalidOperationException("JWT signing key must contain at least 32 UTF-8 bytes.");
}

var jwtOptions = new AuthTokenOptions
{
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "Bakery.Api",
    Audience = builder.Configuration["Jwt:Audience"] ?? "Bakery.Client",
    SigningKey = signingKey,
    AccessTokenMinutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var tokenMinutes)
        ? tokenMinutes
        : 60
};
if (jwtOptions.AccessTokenMinutes is < 1 or > 1440)
{
    throw new InvalidOperationException("JWT access token lifetime must be between 1 and 1440 minutes.");
}

builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtOptions));
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<AdminBootstrapper>();
builder.Services.AddScoped<StartupDataSeeder>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.AdminOnly, policy => policy.RequireRole("Admin"));
    options.AddPolicy(AuthPolicies.CustomerOnly, policy => policy.RequireRole("Customer"));
    options.AddPolicy(
        AuthPolicies.CustomerOrAdmin,
        policy => policy.RequireRole("Customer", "Admin"));
});

var configuredDataDirectory = Environment.GetEnvironmentVariable("BAKERY_DATA_PATH")
    ?? builder.Configuration["Data:DirectoryPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "data");
var dataDirectory = Path.IsPathRooted(configuredDataDirectory)
    ? configuredDataDirectory
    : Path.Combine(builder.Environment.ContentRootPath, configuredDataDirectory);
builder.Services.AddInfrastructure(dataDirectory);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (allowedOrigins.Length > 0)
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    }
}));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AdminBootstrapper>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<StartupDataSeeder>().SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (!string.Equals(builder.Configuration["Swagger:Enabled"], "false", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
