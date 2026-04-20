using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class ContractsConfig : MicroserviceConfig
{
    public override string Name => "contracts";
    public override string ClusterId => "contracts";
    public override string BaseUrl => Environment.GetEnvironmentVariable("CONTRACTS_URL")
        ?? "http://213.136.90.174:6670";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "contracts-root",
            Path = "/contracts",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api" } }
        },
        new MicroserviceRoute
        {
            Name = "contracts-all",
            Path = "/contracts/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/{**catch-all}" } }
        }
    };
}
