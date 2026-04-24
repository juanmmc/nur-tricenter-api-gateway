using ApiGateway.Configuration;
using ApiGateway.Discovery;
using ApiGateway.Observability;
using ApiGateway.Security;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

var observability = ObservabilityOptions.FromEnvironment();
var consulOptions = ConsulOptions.FromEnvironment();

// Serilog: structured JSON logging with trace correlation enrichers.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("service", observability.ServiceName)
    .Enrich.WithProperty("version", observability.ServiceVersion)
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

var keycloakAuthority = Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY")
    ?? "http://207.180.197.169:8080/realms/group3realm";
var keycloakAudience = Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE")
    ?? "account";
var requireHttpsMetadata = bool.TryParse(
    Environment.GetEnvironmentVariable("KEYCLOAK_REQUIRE_HTTPS_METADATA"),
    out var requireHttps) && requireHttps;

const string CorsPolicy = "DefaultCors";

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-Correlation-Id");
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = keycloakAudience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
                    if (realmAccessClaim != null)
                    {
                        var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                        if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var value = role.GetString();
                                if (string.IsNullOrWhiteSpace(value)) continue;
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.Role,
                                    value.ToLowerInvariant()));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Configurar políticas de autorización detalladas
builder.Services.AddAuthorization(options => AuthPolicies.Register(options));

var registry = new MicroserviceRegistry();
builder.Services.AddSingleton(registry);

builder.Services.AddReverseProxy()
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(ctx =>
        {
            ctx.ProxyRequest.Headers.Remove("Origin");
            ctx.ProxyRequest.Headers.Remove("Referer");

            var correlationId = ctx.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                ctx.ProxyRequest.Headers.Remove("X-Correlation-Id");
                ctx.ProxyRequest.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);
            }
            return ValueTask.CompletedTask;
        });
    });

// OpenTelemetry tracing with W3C context propagation to downstream services.
if (observability.TracingEnabled)
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(
                serviceName: observability.ServiceName,
                serviceVersion: observability.ServiceVersion))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Yarp.ReverseProxy")
            .AddZipkinExporter(zipkin =>
            {
                zipkin.Endpoint = new Uri(observability.ZipkinEndpoint);
            }));
}

builder.Services.AddSingleton(consulOptions);
builder.Services.AddSingleton(observability);
builder.Services.AddHttpClient("consul");
builder.Services.AddSingleton<ConsulDiscoveryService>();
builder.Services.AddSingleton<ConsulProxyConfigProvider>();
builder.Services.AddSingleton<Yarp.ReverseProxy.Configuration.IProxyConfigProvider>(
    sp => sp.GetRequiredService<ConsulProxyConfigProvider>());
builder.Services.AddHostedService<ConsulServiceRegistrar>();
builder.Services.AddHostedService<ConsulRoutePoller>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    const string correlationHeader = "X-Correlation-Id";
    var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
        context.Request.Headers[correlationHeader] = correlationId;
    }
    context.Response.Headers[correlationHeader] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("correlation_id", correlationId))
    using (Serilog.Context.LogContext.PushProperty("trace_id", Activity.Current?.TraceId.ToString()))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

app.UseCors(CorsPolicy);

// Prometheus metrics middleware (HTTP request counters + custom metrics).
app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = observability.ServiceName,
    status = "up",
    timestamp = DateTime.UtcNow
}));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Prometheus metrics endpoint scraped by Prometheus.
app.MapMetrics("/metrics");

app.MapControllers();
app.MapReverseProxy();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
