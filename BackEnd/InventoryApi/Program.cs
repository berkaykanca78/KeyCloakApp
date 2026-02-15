using System.Security.Claims;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MassTransit;
using Amazon.S3.Util;
using InventoryApi.Application.Contracts;
using InventoryApi.Application.UseCases;
using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;
using InventoryApi.Infrastructure.Messaging.Consumers;
using InventoryApi.Infrastructure.Persistence;
using InventoryApi.Infrastructure.S3;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 6 * 1024 * 1024; // 6 MB
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});

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
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductDiscountRepository, ProductDiscountRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<GetInventoryPublicUseCase>();
builder.Services.AddScoped<GetAllInventoryUseCase>();
builder.Services.AddScoped<GetInventoryByIdUseCase>();
builder.Services.AddScoped<UpdateQuantityUseCase>();
builder.Services.AddScoped<ReduceStockUseCase>();
builder.Services.AddScoped<CheckAvailabilityUseCase>();
builder.Services.AddScoped<CreateInventoryUseCase>();
builder.Services.AddScoped<CreateProductUseCase>();
builder.Services.AddScoped<CreateProductWithWarehousesUseCase>();
builder.Services.AddScoped<CreateWarehouseUseCase>();
builder.Services.AddScoped<UpdateWarehouseImageUseCase>();
builder.Services.AddScoped<UpdateInventoryImageUseCase>();

var s3Config = builder.Configuration.GetSection("S3");
var s3ServiceUrl = s3Config["ServiceUrl"] ?? "http://localhost:9000";
var bucketName = s3Config["BucketName"] ?? "inventory-images";
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = s3ServiceUrl,
        ForcePathStyle = true,
        AuthenticationRegion = "us-east-1",
    };
    var accessKey = s3Config["AccessKey"];
    var secretKey = s3Config["SecretKey"];
    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
        return new AmazonS3Client(accessKey, secretKey, config);
    return new AmazonS3Client(config);
});
builder.Services.AddScoped<IStorageService>(sp => new S3StorageService(sp.GetRequiredService<IAmazonS3>(), bucketName));

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

using (var scope = app.Services.CreateScope())
{
    // MinIO/S3 bucket yoksa oluştur (ürün resimleri)
    try
    {
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        var bn = builder.Configuration["S3:BucketName"] ?? "inventory-images";
        var exists = await AmazonS3Util.DoesS3BucketExistV2Async(s3, bn);
        if (!exists)
            await s3.PutBucketAsync(new PutBucketRequest { BucketName = bn });
    }
    catch { /* MinIO kapalıysa devam et */ }
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
