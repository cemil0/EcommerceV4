using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using ECommerce.Application.Configuration;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Import;
using ECommerce.Api.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using FluentValidation;
using OfficeOpenXml;

// *** EPPlus 8 License - MUST be at the very top before anything else ***
ExcelPackage.License.SetNonCommercialPersonal("ECommerce Developer");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/ecommerce-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting ECommerce API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("ECommerce.Infrastructure")));

// Register Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStockReservationService, StockReservationService>();
builder.Services.AddScoped<IPriceValidationService, PriceValidationService>();
builder.Services.AddScoped<IOrderStateMachine, OrderStateMachine>();
builder.Services.AddScoped<IOrderBusinessRules, OrderBusinessRules>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
// builder.Services.AddSingleton<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
builder.Services.AddScoped<ICacheWarmupService, CacheWarmupService>();

// Phase 2: New Services
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

// Phase 6: B2B Services
builder.Services.AddScoped<IB2BCreditService, B2BCreditService>();
builder.Services.AddScoped<IB2BDashboardService, B2BDashboardService>();

// Phase 7: Admin Services
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IFileStorageService, AdvancedFileStorageService>();

// Phase 8: Bulk Import Services
builder.Services.AddScoped<IFileParser, ExcelParser>();
builder.Services.AddScoped<IFileParser, CsvParser>();
builder.Services.AddScoped<IBulkProductImportService, BulkProductImportService>();
builder.Services.AddSingleton<BulkImportBackgroundService>();
builder.Services.AddHostedService<BulkImportBackgroundService>(sp => sp.GetRequiredService<BulkImportBackgroundService>());

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(ECommerce.Application.Mappings.MappingProfile));

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add Response Caching
builder.Services.AddResponseCaching();

// ===== REDIS CONFIGURATION (Temporarily Disabled for Local Dev) =====
// var redisConnection = builder.Configuration.GetValue<string>("Redis:ConnectionString");
// 
// builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
// {
//     var configuration = ConfigurationOptions.Parse(redisConnection!);
//     configuration.AbortOnConnectFail = false; // Don't crash if Redis is down
//     configuration.ConnectTimeout = 5000;
//     configuration.SyncTimeout = 5000;
//     
//     return ConnectionMultiplexer.Connect(configuration);
// });

builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("CacheSettings"));

// ===== RATE LIMITING CONFIGURATION =====
// Bind rate limiting configuration
var rateLimitConfig = builder.Configuration
    .GetSection("RateLimiting")
    .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(options =>
{
    // Named policies with partition resolvers
    
    // Auth policy (IP-based, 2 req/min)
    options.AddPolicy("Auth", context =>
    {
        var ipAddress = GetIpAddress(context);
        return RateLimitPartition.GetFixedWindowLimiter(
            $"auth:{ipAddress}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.Policies["Auth"].PermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitConfig.Policies["Auth"].WindowMinutes),
                QueueLimit = 0
            });
    });
    
    // Refresh policy (IP-based, 10 req/min)
    options.AddPolicy("Refresh", context =>
    {
        var ipAddress = GetIpAddress(context);
        return RateLimitPartition.GetFixedWindowLimiter(
            $"refresh:{ipAddress}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.Policies["Refresh"].PermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitConfig.Policies["Refresh"].WindowMinutes),
                QueueLimit = 2
            });
    });
    
    // Orders policy (User-based, 3 req/min)
    options.AddPolicy("Orders", context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(
            $"orders:{userId}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.Policies["Orders"].PermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitConfig.Policies["Orders"].WindowMinutes),
                QueueLimit = 0
            });
    });
    
    // Cart policy (User-based, 20 req/min)
    options.AddPolicy("Cart", context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? GetIpAddress(context);
        return RateLimitPartition.GetFixedWindowLimiter(
            $"cart:{userId}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitConfig.Policies["Cart"].PermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitConfig.Policies["Cart"].WindowMinutes),
                QueueLimit = 3
            });
    });
    
    // Enhanced rejection handler
    options.OnRejected = async (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<Program>>();
        
        var ipAddress = GetIpAddress(context.HttpContext);
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var endpoint = context.HttpContext.Request.Path;
        
        // Structured logging
        logger.LogWarning(
            "RateLimitExceeded | IP={IP} | User={User} | Endpoint={Endpoint}",
            ipAddress, userId ?? "Anonymous", endpoint);
        
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        // Calculate retry-after
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryValue)
            ? (int)retryValue.TotalSeconds
            : 60;
        
        // Enhanced headers
        context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Policy"] = "Auth";
        context.HttpContext.Response.Headers["X-RateLimit-Scope"] = "IP";
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = $"{retryAfter} seconds"
        }, cancellationToken);
    };
});

// Helper method for IP resolution
static string GetIpAddress(HttpContext context)
{
    if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
    return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}
// ===== END RATE LIMITING CONFIGURATION =====

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ECommerceDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"))),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add services to the container

// ===== API VERSIONING =====
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader.Combine(
        new Microsoft.AspNetCore.Mvc.Versioning.UrlSegmentApiVersionReader(),
        new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("X-Api-Version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ===== FLUENT VALIDATION =====
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ECommerce API",
        Version = "v1",
        Description = "E-Commerce platform API for B2C/B2B operations including cart management, order processing, and product catalog",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ECommerce Team",
            Email = "support@ecommerce.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Enable annotations
    options.EnableAnnotations();

    // Add Bearer token authentication
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ECommerceDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        // Seed Data
        try 
        {
            await ECommerce.Infrastructure.Data.DbInitializer.SeedAsync(context);
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
        
        // Cache warmup
        try
        {
            var warmupService = services.GetRequiredService<ICacheWarmupService>();
            await warmupService.WarmupAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache warmup failed, continuing without warmup");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw; // Re-throw to prevent app from starting with broken DB
    }
}

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await ECommerce.Infrastructure.Data.SeedRoles.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}

// Configure the HTTP request pipeline

// Global exception handling (must be first)
// app.UseMiddleware<ECommerce.Api.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

// Response caching
app.UseResponseCaching();

// ===== BLACKLIST MIDDLEWARE (BEFORE RATE LIMITER) =====
app.Use(async (context, next) =>
{
    var ipAddress = GetIpAddress(context);
    if (rateLimitConfig.BlacklistedIPs.Contains(ipAddress))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Access denied" });
        return;
    }
    await next();
});

// ===== RATE LIMITER MIDDLEWARE (AFTER ROUTING, BEFORE AUTHORIZATION) =====
app.UseRateLimiter();

// Enable Swagger in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API V1");
    c.RoutePrefix = "swagger";
});


app.UseHttpsRedirection();

// Serve static files (uploaded images)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Audit Logging Middleware (After Auth)
app.UseMiddleware<ECommerce.Api.Middleware.AdminAuditMiddleware>();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
