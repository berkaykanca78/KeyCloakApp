using GatewayApi;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Request.Mapper;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Authorization'ı downstream'e iletebilmek için: önce özel mapper (AddOcelot TryAdd kullanıyorsa bu kullanılır)
builder.Services.AddSingleton<IRequestMapper, AuthorizationForwardingRequestMapper>();

builder.Services.AddOcelot(builder.Configuration)
    .AddDelegatingHandler<ForwardAuthorizationHandler>(global: true);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors();
// Authorization'ı AsyncLocal'a yaz (DelegatingHandler'da HttpContext bazen null)
app.UseMiddleware<ForwardAuthorizationMiddleware>();
app.UseSwaggerForOcelotUI(opt => { }, uiOpt =>
{
    uiOpt.EnablePersistAuthorization(); // Tek sefer Authorize → token tüm doc'larda kalır (localStorage)
});
app.UseOcelot().Wait();

app.Run();
