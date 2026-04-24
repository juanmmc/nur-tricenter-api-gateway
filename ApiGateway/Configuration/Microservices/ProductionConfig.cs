using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class ProductionConfig : MicroserviceConfig
{
    public override string Name => "production";
    public override string ClusterId => "production";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PRODUCTION_URL")
        ?? "http://207.180.197.169:8000";
    public override bool UseDiscovery => true;
    public override string ConsulServiceName => "microservicio-produccion-cocina";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "production-auth-login",
            Path = "/auth/login",
            Methods = new[] { "POST" },
            AuthorizationPolicy = null,
            CustomTransforms = new() { { "PathPattern", "/api/login" } }
        },
        new MicroserviceRoute
        {
            Name = "production-auth-refresh",
            Path = "/auth/refresh",
            Methods = new[] { "POST" },
            AuthorizationPolicy = null,
            CustomTransforms = new() { { "PathPattern", "/api/refresh" } }
        },
        new MicroserviceRoute
        {
            Name = "production-root",
            Path = "/production",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api" } }
        },
        new MicroserviceRoute
        {
            Name = "production-all",
            Path = "/production/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/{**catch-all}" } }
        }
    };
}
