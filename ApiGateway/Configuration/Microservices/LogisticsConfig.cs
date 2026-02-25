using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class LogisticsConfig : MicroserviceConfig
{
    public override string Name => "logistics";
    public override string ClusterId => "logistics";
    public override string BaseUrl => Environment.GetEnvironmentVariable("LOGISTICS_URL")
        ?? "http://localhost:8090";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "logistics-root",
            Path = "/logistics",
            Methods = ["GET", "POST", "PUT", "DELETE", "PATCH"],
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api" } }
        },
        new MicroserviceRoute
        {
            Name = "logistics-all",
            Path = "/logistics/{**catch-all}",
            Methods = ["GET", "POST", "PUT", "DELETE", "PATCH"],
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/{**catch-all}" } }
        }
    };
}