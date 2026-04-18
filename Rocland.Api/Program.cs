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

// ─────────────────────────────────────────────────────────────────────────────
// LOGGING TEMPRANO
// Se configura antes del builder para capturar errores de arranque.
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/accesocontrol-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ─────────────────────────────────────────────────────────────────────────────
// CONFIGURACIÓN DE MÓDULOS
// Cada módulo puede tener su propio appsettings.json y appsettings.Development.json
// dentro de su subcarpeta en /Modules. Ambos archivos se fusionan con la
// configuración principal. El archivo .Development.json sobreescribe al base,
// igual que en una app ASP.NET Core estándar.
//
// Estructura esperada por módulo:
//   Modules/
//     AccesoControl/
//       appsettings.json              ← valores base (sin secretos, commiteado)
//       appsettings.Development.json  ← secretos y overrides locales (en .gitignore)
//
// Para añadir un nuevo módulo: solo coloca su DLL y sus appsettings en
// una carpeta dentro de /Modules. El host lo descubrirá automáticamente.
// ─────────────────────────────────────────────────────────────────────────────
var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
Directory.CreateDirectory(modulesPath);

var environmentName = builder.Environment.EnvironmentName; // "Development", "Production", etc.

foreach (var moduleDir in Directory.GetDirectories(modulesPath))
{
    var moduleName = Path.GetFileName(moduleDir);

    // 1. Configuración base del módulo (sin secretos)
    var baseConfig = Path.Combine(moduleDir, "appsettings.json");
    if (File.Exists(baseConfig))
    {
        builder.Configuration.AddJsonFile(baseConfig, optional: true, reloadOnChange: true);
        Log.Information("[Host] ⚙️ Config base cargada: {Module}", moduleName);
    }

    // 2. Configuración de entorno del módulo (secretos y overrides por ambiente)
    //    Equivalente a appsettings.Development.json de la app principal.
    var envConfig = Path.Combine(moduleDir, $"appsettings.{environmentName}.json");
    if (File.Exists(envConfig))
    {
        builder.Configuration.AddJsonFile(envConfig, optional: true, reloadOnChange: true);
        Log.Information("[Host] 🔒 Config de entorno cargada: {Module} ({Env})", moduleName, environmentName);
    }

    // Se inyecta en el despliegue x
    var secretsConfig = Path.Combine(moduleDir, "appsettings.Secrets.json");
    if (File.Exists(secretsConfig))
    {
        builder.Configuration.AddJsonFile(secretsConfig, optional: true, reloadOnChange: true);
        Log.Information("[Host] 🔑 Secretos cargados para el módulo: {Module}", moduleName);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DESCUBRIMIENTO DINÁMICO DE MÓDULOS (DLLs)
// Se buscan y cargan todos los ensamblados que implementen IRoclandModule.
// Para añadir un módulo: solo coloca su DLL en una subcarpeta de /Modules.
// ─────────────────────────────────────────────────────────────────────────────
var moduleAssemblies = Directory
    .GetFiles(modulesPath, "RCD.*.Module.dll", SearchOption.AllDirectories)
    .Select(Assembly.LoadFrom)
    .ToList();

var modules = new List<IRoclandModule>();

foreach (var assembly in moduleAssemblies)
{
    var moduleTypes = assembly.GetTypes()
        .Where(t => typeof(IRoclandModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

    foreach (var type in moduleTypes)
    {
        if (Activator.CreateInstance(type) is IRoclandModule instance)
        {
            modules.Add(instance);
            Log.Information("[Host] 📦 Módulo cargado: {Name} v{Version}", instance.Name, instance.Version);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// INFRAESTRUCTURA COMPARTIDA DEL HOST
// Servicios globales que todos los módulos heredan.
// ─────────────────────────────────────────────────────────────────────────────

// SignalR (usado por AccesoControl y potencialmente otros módulos)
builder.Services.AddSignalR();

// Autenticación JWT
// La clave y el issuer viven en appsettings.Development.json del host (o de un módulo de auth).
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

    // Permite que SignalR reciba el token vía query string (?access_token=...)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.Value?.Contains("accesohub", StringComparison.OrdinalIgnoreCase) == true)
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Política base: solo requiere usuario autenticado.
    // Para restringir por módulo/proyecto en el futuro, descomenta:
    // policy.RequireClaim("ProyectosAsignados", "AccesoControlWeb");
    options.AddPolicy("AccesoControlWebPolicy", policy =>
    policy.RequireAuthenticatedUser()
          .RequireRole("Admin", "Supervisor")
          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));

    options.AddPolicy("AccesoControlMobilePolicy", policy =>
    policy.RequireAuthenticatedUser()
          .RequireRole("Guardia")
          .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme));
});

// Swagger
// Cada módulo declara su propio grupo (ApiExplorerSettings GroupName).
// Para añadir un módulo al Swagger: agrega un SwaggerDoc y un endpoint abajo.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("web-accesocontrol", new OpenApiInfo
    {
        Title = "Rocland API — Acceso Control Web",
        Version = "v1",
        Description = "Módulo de control de accesos (plataforma web)"
    });

    // Para agregar un nuevo módulo al Swagger:
    c.SwaggerDoc("mobile-accesocontrol", new OpenApiInfo
    {
        Title = "Rocland API — Acceso Control Mobile",
        Version = "v1",
        Description = "Módulo de control de accesos (app móvil guardias)"
    });

    c.ResolveConflictingActions(descriptions => descriptions.First());

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out _)) return false;
        return apiDesc.GroupName == docName;
    });

    // Soporte de token JWT en la UI de Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa: Bearer {tu_token}",
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Proxy inverso (Caddy / Nginx en Docker)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// HSTS (solo activo en producción, ver middleware pipeline más abajo)
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// CORS — política permisiva para la app móvil
// Para producción, considera restringir los orígenes permitidos.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPanelPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:4000"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());

    options.AddPolicy("MobilePolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Rate limiting — protege formularios y endpoints sensibles
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("FormSubmissionLimit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(3);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

// ─────────────────────────────────────────────────────────────────────────────
// INYECCIÓN DE DEPENDENCIAS DE MÓDULOS
// Cada módulo registra sus propios servicios, DbContexts y repositorios.
// ─────────────────────────────────────────────────────────────────────────────
foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}

// ─────────────────────────────────────────────────────────────────────────────
// MEDIATR — BUS DE EVENTOS
// Se registran los handlers de la API host y de todos los módulos.
// ─────────────────────────────────────────────────────────────────────────────
var allAssemblies = new List<Assembly> { typeof(Program).Assembly };
allAssemblies.AddRange(moduleAssemblies);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(allAssemblies.ToArray()));

// ─────────────────────────────────────────────────────────────────────────────
// PIPELINE DE MIDDLEWARE
// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/web-accesocontrol/swagger.json", "Acceso Control Web");

        c.SwaggerEndpoint("/swagger/mobile-accesocontrol/swagger.json", "Acceso Control Mobile");

        // Para agregar un nuevo módulo al Swagger UI:
        // c.SwaggerEndpoint("/swagger/web-inventario/swagger.json", "Inventario");
    });
}

app.UseRateLimiter();
app.UseHttpsRedirection();

// Headers de seguridad HTTP
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
    app.UseHsts();

app.UseRouting();   

app.UseCors("AngularPanelPolicy");  

app.UseAuthentication();
app.UseAuthorization();     

// Cada módulo registra sus propios endpoints, hubs de SignalR y Razor Pages
foreach (var module in modules)
{
    module.ConfigureApplication(app);
}

app.UseSerilogRequestLogging(options =>
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0}ms");

app.MapControllers();

// ─────────────────────────────────────────────────────────────────────────────
// MIGRACIONES AUTOMÁTICAS
// Se ejecutan al arranque para todos los DbContexts registrados por módulos.
// ─────────────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContextTypes = moduleAssemblies
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract);

    foreach (var dbContextType in dbContextTypes)
    {
        try
        {
            if (scope.ServiceProvider.GetRequiredService(dbContextType) is DbContext dbContext)
            {
                Log.Information("[Host] 🗄️ Migrando base de datos: {DbContext}", dbContextType.Name);
                dbContext.Database.Migrate();
                Log.Information("[Host] ✔️ Migración completada: {DbContext}", dbContextType.Name);
            }
        }
        catch (Exception ex)
        {
            Log.Error("[Host] ❌ Error migrando {DbContext}: {Message}", dbContextType.Name, ex.Message);
        }
    }
}

app.Run();