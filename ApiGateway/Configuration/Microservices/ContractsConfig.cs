using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class ContractsConfig : MicroserviceConfig
{
    public override string Name => "contrato";
    public override string ClusterId => "contrato";
    public override string BaseUrl => Environment.GetEnvironmentVariable("CONTRATO_URL")
        ?? "http://213.136.90.174:6670";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "contrato-root",
            Path = "/contrato",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api" } }
        },
        new MicroserviceRoute
        {
            Name = "contrato-all",
            Path = "/contrato/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/{**catch-all}" } }
        }
    };
}
