using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Identity.API.Services;

var builder = WebApplication.CreateBuilder(args);

var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var config = ConfigurationOptions.Parse(redisConnection);
    return ConnectionMultiplexer.Connect(config);
});
builder.Services.AddScoped<ICityRedisService, CityRedisService>();
builder.Services.AddScoped<IKeycloakAdminService, KeycloakAdminService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var keycloak = builder.Configuration.GetSection("Keycloak");
var authority = keycloak["Authority"] ?? "http://localhost:8080/realms/KeyCloakApp";
var audience = keycloak["Audience"] ?? keycloak["ClientId"] ?? "backend-api";
var requireHttps = keycloak.GetValue<bool>("RequireHttpsMetadata");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = requireHttps;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authority.TrimEnd('/'),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "preferred_username",
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity?)context.Principal?.Identity;
                if (identity == null) return Task.CompletedTask;
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccess))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var roles))
                            foreach (var role in roles.EnumerateArray())
                            {
                                var roleValue = role.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                            }
                    }
                    catch { /* ignore */ }
                }
                foreach (var claim in context.Principal!.FindAll("role").ToList())
                {
                    if (!string.IsNullOrEmpty(claim.Value) && !identity.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == claim.Value))
                        identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Önce /api/auth/login ile token alın, buraya yapıştırın.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var cityService = scope.ServiceProvider.GetRequiredService<ICityRedisService>();
        await cityService.SeedAsync();
    }
    catch (Exception)
    {
        // Redis kapalıysa veya seed hata verirse uygulama yine de ayağa kalksın
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity.API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
