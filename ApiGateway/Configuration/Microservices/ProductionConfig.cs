using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class ProductionConfig : MicroserviceConfig
{
    public override string Name => "production";
    public override string ClusterId => "production";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PRODUCTION_URL")
        ?? "http://localhost:8000";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
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
