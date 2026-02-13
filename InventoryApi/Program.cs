using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MassTransit;
using InventoryApi.Application.UseCases;
using InventoryApi.Infrastructure.Messaging.Consumers;
using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;
using InventoryApi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<GetInventoryPublicUseCase>();
builder.Services.AddScoped<GetAllInventoryUseCase>();
builder.Services.AddScoped<GetInventoryByIdUseCase>();
builder.Services.AddScoped<UpdateQuantityUseCase>();
builder.Services.AddScoped<ReduceStockUseCase>();
builder.Services.AddScoped<CheckAvailabilityUseCase>();

var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReserveStockConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMq["Host"] ?? "localhost", "/", h =>
        {
            h.Username(rabbitMq["Username"] ?? "guest");
            h.Password(rabbitMq["Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InventoryApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. AuthApi /api/auth/login ile token alın.",
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

var keycloak = builder.Configuration.GetSection("Keycloak");
var authority = keycloak["Authority"] ?? "http://localhost:8080/realms/KeyCloakApp";
var audience = keycloak["Audience"] ?? "backend-api";
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
            NameClaimType = "preferred_username", // Keycloak kullanıcı adı bu claim'de gelir (User.Identity.Name için)
        };
        // Keycloak varsayılan token'da roller realm_access.roles içinde gelir; bunları Role claim'ine çeviriyoruz
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity?)context.Principal?.Identity;
                if (identity == null) return Task.CompletedTask;

                // 1) realm_access.roles (Keycloak varsayılan)
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
                    catch { /* ignore parse error */ }
                }

                // 2) Ayrı "role" claim'leri (mapper kullanılıyorsa)
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

var app = builder.Build();

app.UseCors();

// Hiç kayıt yoksa seed data ekle (DDD: domain factory kullanılır)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    if (!await db.InventoryItems.AnyAsync())
    {
        await db.InventoryItems.AddRangeAsync(
            InventoryItem.CreateForSeed("Ürün A", 100, "Depo-1"),
            InventoryItem.CreateForSeed("Ürün B", 50, "Depo-1"),
            InventoryItem.CreateForSeed("Ürün C", 200, "Depo-2"));
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryApi v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
