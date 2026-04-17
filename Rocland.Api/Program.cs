using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using RCD.Shared.Kernel.Modularity;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/accesocontrol-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ====================================================================
// 🚀 INICIO DEL DESCUBRIMIENTO DE CONFIGURACIONES (MÓDULOS)
// ====================================================================
var modulesPathConfig = Path.Combine(AppContext.BaseDirectory, "Modules");
if (!Directory.Exists(modulesPathConfig))
{
    Directory.CreateDirectory(modulesPathConfig);
}

// Buscar cualquier archivo que se llame appsettings.json o appsettings.*.json dentro de Modules
var moduleConfigFiles = Directory.GetFiles(modulesPathConfig, "appsettings.Secrets*.json", SearchOption.AllDirectories);

foreach (var configFile in moduleConfigFiles)
{
    // Fusionamos la configuración del módulo con la principal de la API
    builder.Configuration.AddJsonFile(configFile, optional: true, reloadOnChange: true);

    // Solo para visualizar qué está cargando:
    var nombreCarpetaModulo = Path.GetFileName(Path.GetDirectoryName(configFile));
    Console.WriteLine($"[Rocland Host] ⚙️ Configuración cargada del módulo: {nombreCarpetaModulo}");
}
// ====================================================================
// FIN DEL DESCUBRIMIENTO DE CONFIGURACIONES
// ====================================================================

// ── MVC + Razor Pages ──────────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ── SignalR ────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── JWT Auth ───────────────────────────────────────────────────────────
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Soporte JWT para SignalR (el token viaja en query string)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // El cambio clave: usar Contains e ignorar mayúsculas
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.Value != null &&
                    path.Value.Contains("accesohub", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
     .AddCookie("AdminCookie", options =>
     {
         options.LoginPath = "/Admin/Login";
         options.LogoutPath = "/Admin/Logout";
         options.AccessDeniedPath = "/Admin/Login";
         options.ExpireTimeSpan = TimeSpan.FromHours(8);
         options.SlidingExpiration = true;
         options.Cookie.Name = "RoclandAdmin";
         options.Cookie.HttpOnly = true;
         options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
     }); ;

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccesoControlWebPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();

        // MAGIA PARA EL FUTURO:
        // Cuando tengas muchos proyectos, descomentarás la siguiente línea.
        // Esto obligará a que el token JWT del usuario contenga un permiso explícito 
        // para entrar a Acceso Control. Si entra alguien de "Inventario", el sistema lo rechazará.
        // policy.RequireClaim("ProyectosAsignados", "AccesoControlWeb"); 
    });
});

// ── Swagger ────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 1. Definir el documento para el proyecto actual
    c.SwaggerDoc("web-accesocontrol", new OpenApiInfo
    {
        Title = "Rocland API - Acceso Control Web",
        Version = "v1",
        Description = "Módulo para el sistema de control de accesos (Plataforma Web)"
    });

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

        // Si el documento es 'web-accesocontrol', solo incluye controladores con ese GroupName
        if (docName == "web-accesocontrol")
        {
            return apiDesc.GroupName == "web-accesocontrol";
        }

        return true;
    });

    // En el futuro, aquí agregarás los demás:
    // c.SwaggerDoc("web-inventario", new OpenApiInfo { Title = "Rocland API - Inventario", Version = "v1" });

    // 2. Configurar la interfaz de Swagger para que acepte el Token JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT. Escribe 'Bearer' [espacio] y luego tu token.\r\n\r\nEjemplo: 'Bearer 12345abcdef'",
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

// Configuración para Proxy Inverso (Caddy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // IMPORTANTE: Como estamos en una red interna de Docker, limpiamos las redes conocidas
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ── Seguridad: Headers HTTP ────────────────────────────────────────────
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// ── Antiforgery para Razor Pages ───────────────────────────────────────
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "RoclandXSRF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// ── CORS (para la app móvil) ───────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobilePolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Configuración de Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FormSubmissionLimit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(3);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429; // Too Many Requests
});

// -- Quest ----------------------------------
QuestPDF.Settings.License = LicenseType.Community;

// ====================================================================
// 🚀 INICIO DEL DESCUBRIMIENTO DINÁMICO DE MÓDULOS
// ====================================================================
var modules = new List<IRoclandModule>();

// 1. Definir la ruta de la carpeta Modules
var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
if (!Directory.Exists(modulesPath))
{
    Directory.CreateDirectory(modulesPath);
}

// 2. Buscar TODAS las DLLs en la carpeta Modules (buscando en subcarpetas)
var moduleAssemblies = Directory.GetFiles(modulesPath, "RCD.*.Module.dll", SearchOption.AllDirectories)
                                .Select(Assembly.LoadFrom)
                                .ToList();

// 2. Extraer las clases que implementen IRoclandModule
foreach (var assembly in moduleAssemblies)
{
    var moduleTypes = assembly.GetTypes()
        .Where(t => typeof(IRoclandModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

    foreach (var type in moduleTypes)
    {
        // Instanciar la clase del módulo dinámicamente
        if (Activator.CreateInstance(type) is IRoclandModule moduleInstance)
        {
            modules.Add(moduleInstance);
            Console.WriteLine($"[Rocland Host] Módulo descubierto y cargado: {moduleInstance.Name} v{moduleInstance.Version}");
        }
    }
}

// 3. Ejecutar la fase de Inyección de Dependencias (ConfigureServices) de todos los módulos
foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}
// ====================================================================
// FIN DEL DESCUBRIMIENTO
// ====================================================================

// ====================================================================
// 🚀 INICIO DE CONFIGURACIÓN DEL BUS DE EVENTOS (MediatR)
// ====================================================================
// Recopilamos el assembly de la API y todos los assemblies de los módulos descubiertos
var allAssemblies = new List<Assembly> { typeof(Program).Assembly };
allAssemblies.AddRange(moduleAssemblies);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(allAssemblies.ToArray());
});
// ====================================================================
// FIN DE CONFIGURACIÓN DEL BUS
// ====================================================================

var app = builder.Build();

app.UseForwardedHeaders();

// ── Middleware pipeline ────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // 1. Apuntamos al documento que creamos arriba
        c.SwaggerEndpoint("/swagger/web-accesocontrol/swagger.json", "Web - Acceso Control");

        // En el futuro, agregarás los demás aquí y aparecerán en un menú desplegable:
        // c.SwaggerEndpoint("/swagger/web-inventario/swagger.json", "Web - Inventario");
    });
}

app.UseRateLimiter();

app.UseHttpsRedirection();

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors("MobilePolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

foreach (var module in modules)
{
    module.ConfigureApplication(app);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0}ms";
});

// ====================================================================
// 🚀 INICIO DE MIGRACIONES DINÁMICAS
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    // Buscamos todas las clases en las DLLs de los módulos que hereden de DbContext
    var dbContextTypes = moduleAssemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract);

    foreach (var dbContextType in dbContextTypes)
    {
        try
        {
            // Extraemos el DbContext del contenedor de inyección de dependencias
            if (scope.ServiceProvider.GetRequiredService(dbContextType) is DbContext dbContext)
            {
                Console.WriteLine($"[Rocland Host] Verificando migraciones para la base de datos de: {dbContextType.Name}...");

                // Ejecutamos la migración de forma síncrona/bloqueante (necesario en el arranque)
                dbContext.Database.Migrate();

                Console.WriteLine($"[Rocland Host] ✔️ Base de datos actualizada para: {dbContextType.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Rocland Host] ❌ Error al migrar {dbContextType.Name}: {ex.Message}");
        }
    }
}
// ====================================================================
// FIN DE MIGRACIONES
// ====================================================================

// Ejecutar la aplicación
app.Run();
