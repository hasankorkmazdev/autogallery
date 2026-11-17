using FatihTasci.Core.Converters;
using FatihTasci.Core.Entity.Identity;
using FatihTasci.Core.Extensions.Data;
using FatihTasci.Core.Logging;
using FatihTasci.Core.Middleware;
using FatihTasci.Core.Models.Domain;
using FatihTasci.Core.Models.Domain.Common;
using FatihTasci.Core.Providers;
using FatihTasci.Core.Service.Domains;
using FatihTasci.Core.Service.Domains.Abstracts;
using FatihTasci.Data.Context;
using FatihTasci.Server.Helpers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

public class Program
{
    public const string LoginRateLimitPolicy = "login";
    public const string FormRateLimitPolicy = "forms";
    public const long MaxUploadBytes = 300L * 1024 * 1024;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddAppSerilog();

        var turkish = new CultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = turkish;
        CultureInfo.DefaultThreadCurrentUICulture = turkish;
        ValidatorOptions.Global.LanguageManager.Culture = turkish;

        builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);

        ConfigureServices(builder.Services, builder.Configuration);

        // Visual Studio "Panel" profili: sunucuyla birlikte Vite'ı da başlatır (yalnız geliştirmede).
        if (builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("DevTools:StartViteDevServer"))
            builder.Services.AddHostedService<ViteDevServer>();

        WebApplication app;
        try
        {
            app = builder.Build();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Server ayağa kaldırılırken hata oluştu");
            Log.CloseAndFlush();
            throw;
        }

        ConfigureMiddleware(app);
        MigrateDatabase(app);

        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Server beklenmedik şekilde sonlandı");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(x => x.UseSqlServer(connectionString)).AddUnitOfWork<AppDbContext>();

        services.AddControllersWithViews()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                x.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                x.JsonSerializerOptions.Converters.Add(new FlexibleEnumConverterFactory());
                x.JsonSerializerOptions.Converters.Add(new BoolConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Doğrulama hataları da ServiceResultModel zarfıyla döner: Data = { alan: [mesajlar] }
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            x => ToCamelCase(x.Key),
                            x => x.Value.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Değer geçersiz." : e.ErrorMessage).ToArray());

                    return new BadRequestObjectResult(new ServiceResultModel<Dictionary<string, string[]>>
                    {
                        Code = 400,
                        Message = errors.Values.SelectMany(x => x).FirstOrDefault() ?? "Gönderilen bilgiler geçersiz.",
                        Data = errors
                    });
                };
            });

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSingleton<ICurrentUserProvider, HttpContextCurrentUserProvider>();
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddSingleton<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
        services.AddGlobalExceptionHandler();
        services.AddServiceResultResponseProccesor();
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<IRequestModel>();

        AddAuthentication(services, configuration);
        AddRateLimiting(services);
        AddBusinessServices(services);
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"];
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new InvalidOperationException("JwtSettings:Secret ayarı en az 32 karakter olmalıdır.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
                options.Events = new JwtBearerEvents
                {
                    // Token yalnız HttpOnly oturum çerezinden okunur; çerez yoksa Authorization başlığına düşülmez.
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies[AuthCookie.Name];
                        if (string.IsNullOrEmpty(token))
                            context.NoResult();
                        else
                            context.Token = token;
                        return Task.CompletedTask;
                    },
                    // Pasife alınan ya da rolü değişen kullanıcının eski token'ı reddedilir (panel girişe döner).
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var userIdValue = principal?.FindFirst(HttpContextCurrentUserProvider.UserIdClaim)?.Value;
                        var role = principal?.FindFirst(ClaimTypes.Role)?.Value;
                        var accountService = context.HttpContext.RequestServices.GetRequiredService<IAccountService>();
                        if (!Guid.TryParse(userIdValue, out var userId) || !await accountService.IsSessionValidAsync(userId, role))
                            context.Fail("Oturum geçersiz.");
                    },
                    // Rol yetmediğinde panel toast'ında gösterilecek mesaj, ServiceResultModel zarfıyla
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new ServiceResultModel
                        {
                            Code = StatusCodes.Status403Forbidden,
                            Message = "Bu işlem için yetkiniz yok."
                        });
                    }
                };
            });
        services.AddAuthorization();
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await context.HttpContext.Response.WriteAsync("Çok fazla istek gönderdiniz. Lütfen biraz sonra tekrar deneyin.", token);
            };

            // Panel girişi: IP başına dakikada 10 deneme
            options.AddPolicy(LoginRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));

            // Vitrin formları: IP başına 10 dakikada 5 gönderim
            options.AddPolicy(FormRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10) }));
        });
    }

    private static void AddBusinessServices(IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ISiteSettingService, SiteSettingService>();
        services.AddScoped<ISiteContentService, SiteContentService>();
        services.AddScoped<IShowroomService, ShowroomService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IPublicVehicleService, PublicVehicleService>();
        services.AddSingleton<IMediaRegenerationRunner, MediaRegenerationRunner>();
        services.AddSingleton<IShareImageService, ShareImageService>();
        services.AddSingleton<SiteUrlBuilder>();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseSecurityHeaders();
        app.UseServiceResultResponseProccesor();
        app.UseGlobalExceptionHandler();

        app.UseSerilogRequestLogging(logging =>
        {
            logging.MessageTemplate = "{RequestMethod} {RequestPath} -> {StatusCode} ({Elapsed:0} ms)";
            logging.GetLevel = (httpContext, _, exception) =>
            {
                if (exception is not null || httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                var path = httpContext.Request.Path;
                if (path.StartsWithSegments("/media") || path.StartsWithSegments("/css") || path.StartsWithSegments("/js") || path.StartsWithSegments("/panel/assets"))
                    return LogEventLevel.Verbose;

                return LogEventLevel.Information;
            };
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/hata");
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Vitrin sayfalarında 404 vb. için hata sayfası (API, medya ve panel hariç)
        app.UseWhen(
            context => !IsApiOrAsset(context.Request.Path),
            branch => branch.UseStatusCodePagesWithReExecute("/hata/{0}"));

        // Panelin router basename'i "/panel/": sondaki eğik çizgi olmadan gelen istek yönlendirilir, yoksa panel boş açılır.
        // (Routing sondaki eğik çizgiyi yok saydığı için endpoint değil, tam yol karşılaştırması.)
        app.Use((context, next) =>
        {
            if (string.Equals(context.Request.Path.Value, "/panel", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/panel/" + context.Request.QueryString, permanent: true);
                return Task.CompletedTask;
            }
            return next(context);
        });

        app.UseStaticFiles();

        var storage = app.Services.GetRequiredService<IMediaStorage>();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.PublicRoot),
            RequestPath = LocalMediaStorage.RequestPath,
            OnPrepareResponse = context =>
            {
                // Dosya adları benzersiz (guid) olduğu için uzun süre önbelleğe alınabilir.
                context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            }
        });

        app.UseApiCsrfProtection();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Yönetim paneli (React, derlenmiş hali wwwroot/panel'de)
        app.MapFallbackToFile("/panel/{**path}", "panel/index.html");
    }

    private static bool IsApiOrAsset(PathString path)
    {
        return path.StartsWithSegments("/api") || path.StartsWithSegments("/media") || path.StartsWithSegments("/panel");
    }

    private static void MigrateDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        context.Database.Migrate();
        // Örnek teslimat ve araçlar yalnız geliştirmede (stok / başka galerinin fotoğrafları canlıda gerçek gibi görünür).
        var demoSeedFolder = app.Environment.IsDevelopment()
            ? Path.Combine(app.Environment.ContentRootPath, "Assets", "Seed")
            : null;
        DbSeeder.SeedAsync(
                context,
                app.Configuration,
                scope.ServiceProvider.GetRequiredService<IPasswordHasher<UserEntity>>(),
                scope.ServiceProvider.GetRequiredService<IMediaStorage>(),
                demoSeedFolder,
                logger)
            .GetAwaiter()
            .GetResult();
    }

    private static string ToCamelCase(string key)
    {
        if (string.IsNullOrEmpty(key))
            return key;

        return string.Join(".", key.Split('.').Select(part => part.Length == 0 ? part : char.ToLowerInvariant(part[0]) + part[1..]));
    }
}
