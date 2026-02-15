using Gateway.API;
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
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            .WithExposedHeaders("X-Basket-Id");
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
    uiOpt.EnablePersistAuthorization();
    // Tek doc'ta Authorize yapılınca token tüm doc'lara yayılsın; 401 ve tekrar token girişi önlenir
    uiOpt.HeadContent = """
        <script>
        (function() {
            function getBearerAuth(auth) {
                if (!auth || typeof auth !== 'object') return null;
                var b = auth.Bearer || auth.bearer;
                if (!b) return null;
                var val = (b.value !== undefined && b.value !== '') ? b.value : (b.token || b);
                if (typeof val === 'string' && val.length > 0) return auth;
                if (b.name) return auth;
                return null;
            }
            function syncOneObject(data) {
                if (!data || typeof data !== 'object') return false;
                var keys = Object.keys(data);
                if (keys.length === 0) return false;
                var sourceAuth = null;
                for (var i = 0; i < keys.length; i++) {
                    sourceAuth = getBearerAuth(data[keys[i]]);
                    if (sourceAuth) break;
                }
                if (!sourceAuth) return false;
                var changed = false;
                for (var j = 0; j < keys.length; j++) {
                    if (!getBearerAuth(data[keys[j]])) { data[keys[j]] = sourceAuth; changed = true; }
                }
                return changed;
            }
            function runSync() {
                try {
                    var raw = localStorage.getItem('authorized');
                    if (!raw) return;
                    var data = JSON.parse(raw);
                    if (syncOneObject(data)) {
                        localStorage.setItem('authorized', JSON.stringify(data));
                    }
                } catch (e) {}
                var prefix = 'authorized';
                for (var i = 0; i < localStorage.length; i++) {
                    var k = localStorage.key(i);
                    if (k && k !== 'authorized' && k.indexOf(prefix) === 0) {
                        try {
                            var r = localStorage.getItem(k);
                            if (r) {
                                var d = JSON.parse(r);
                                if (syncOneObject(d)) localStorage.setItem(k, JSON.stringify(d));
                            }
                        } catch (e) {}
                    }
                }
            }
            var origSetItem = localStorage.setItem.bind(localStorage);
            localStorage.setItem = function(key, value) {
                if (key === 'authorized' && typeof value === 'string') {
                    try {
                        var data = JSON.parse(value);
                        if (syncOneObject(data)) value = JSON.stringify(data);
                    } catch (e) {}
                }
                return origSetItem(key, value);
            };
            runSync();
            setTimeout(runSync, 200);
            setTimeout(runSync, 500);
            setTimeout(runSync, 1000);
            var t = setInterval(runSync, 800);
            setTimeout(function() { clearInterval(t); }, 45000);
        })();
        </script>
        """;
});
app.UseOcelot().Wait();

app.Run();
