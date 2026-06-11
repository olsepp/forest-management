using App.BLL;
using App.BLL.Services.Interfaces;
using App.BLL.Services.Implementations;
using App.DAL.EF;
using App.DAL.EF.DataSeeding;
using App.DAL.UnitOfWork;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using System.Collections.Concurrent;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Database
// -----------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------------------------------------------------------------
// Identity (used for JWT auth only - no UI needed)
// ---------------------------------------------------------------------
builder.Services
    .AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// -----------------------------------------------------------------------
// JWT Authentication
// -----------------------------------------------------------------------
// -----------------------------------------------------------------------
// JWT Settings — bind configuration section so AuthService can inject it
// -----------------------------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));

var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT configuration section is missing.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
    throw new InvalidOperationException("JWT:Key is not configured.");

// -----------------------------------------------------------------------
// Authentication — do NOT set a global default scheme so that
// ASP.NET Identity cookie auth (used by MVC Admin area) continues to work.
// API controllers explicitly request JwtBearer via [Authorize(AuthenticationSchemes=...)]
// on ApiControllerBase.
// -----------------------------------------------------------------------
builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

// -----------------------------------------------------------------------
// CORS - Allow SvelteKit dev server and production frontend
// -----------------------------------------------------------------------
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "";
var origins = new List<string>
{
    "http://localhost:5173",
    "http://localhost:4173",
    "http://localhost:3000"
};
if (!string.IsNullOrWhiteSpace(frontendUrl))
{
    origins.Add(frontendUrl);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(origins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// -----------------------------------------------------------------------
// Rate Limiting — in-memory login attempt tracker per IP
// Custom middleware because the built-in ASP.NET Core rate limiters
// (FixedWindow/TokenBucket) have a known bug (dotnet/runtime#92557)
// where RetryAfter metadata always returns the full window duration,
// not the remaining time. This approach gives exact countdown.
// -----------------------------------------------------------------------
var loginAttempts = new ConcurrentDictionary<string, ConcurrentQueue<DateTime>>();

_ = new Timer(state =>
{
    var now = DateTime.UtcNow;
    var window = TimeSpan.FromMinutes(15);
    foreach (var kvp in loginAttempts)
    {
        while (kvp.Value.TryPeek(out DateTime oldest) && (now - oldest) > window)
        {
            kvp.Value.TryDequeue(out _);
        }
        if (kvp.Value.IsEmpty)
        {
            loginAttempts.TryRemove(kvp.Key, out _);
        }
    }
}, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

// -----------------------------------------------------------------------
// DI — UnitOfWork & BLL Services
// -----------------------------------------------------------------------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ILandPropertyService, LandPropertyService>();
builder.Services.AddScoped<ICadasterService, CadasterService>();
builder.Services.AddScoped<IForestStandService, ForestStandService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IActivityTypeService, ActivityTypeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IActivityExportService, ActivityExportService>();

// -----------------------------------------------------------------------
// API Controllers Only
// -----------------------------------------------------------------------
builder.Services.AddControllers();

// -----------------------------------------------------------------------
// Swagger / OpenAPI
// -----------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Forest Management API",
        Version = "v1",
        Description = "REST API for Forest Management System"
    });

    // Add JWT Bearer auth to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
});

// -----------------------------------------------------------------------
// Forwarded headers (trust nginx reverse proxy in Docker)
// -----------------------------------------------------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// -----------------------------------------------------------------------
// Build
// -----------------------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------------------
// Apply pending EF Core migrations, then seed
// -----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await DataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred applying migrations or seeding the database.");
    }
}

// -----------------------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------------------
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Forest Management API v1");
        options.RoutePrefix = "swagger";
    });
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Internal server error.\"}");
        });
    });
    app.UseHsts();
}
app.UseRouting();

// CORS must be after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors("FrontendPolicy");

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/api/account/login" && context.Request.Method == "POST")
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var attempts = loginAttempts.GetOrAdd(ip, _ => new ConcurrentQueue<DateTime>());
        var now = DateTime.UtcNow;
        var window = TimeSpan.FromMinutes(15);

        while (attempts.TryPeek(out var oldest) && (now - oldest) > window)
        {
            attempts.TryDequeue(out _);
        }

        if (attempts.Count >= 5)
        {
            attempts.TryPeek(out var first);
            var remaining = (first + window) - now;
            var totalSeconds = Math.Max((int)remaining.TotalSeconds, 1);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Retry-After"] = totalSeconds.ToString();

            string message;
            if (totalSeconds >= 60)
            {
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                message = seconds > 0
                    ? $"Liiga palju katseid. Proovige uuesti {minutes} minuti ja {seconds} sekundi pärast."
                    : $"Liiga palju katseid. Proovige uuesti {minutes} minuti pärast.";
            }
            else
            {
                message = $"Liiga palju katseid. Proovige uuesti {totalSeconds} sekundi pärast.";
            }

            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new { message }));
            return;
        }

        attempts.Enqueue(now);
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.Run();
