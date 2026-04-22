using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class PlansConfig : MicroserviceConfig
{
    public override string Name => "plans";
    public override string ClusterId => "plans";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PLANS_URL")
        ?? "http://142.93.120.239:8081";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "meal-plans-root",
            Path = "/meal-plans",
            Methods = new[] { "GET", "POST" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/meal-plans" } }
        },
        new MicroserviceRoute
        {
            Name = "meal-plans-all",
            Path = "/meal-plans/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/meal-plans/{**catch-all}" } }
        },
        new MicroserviceRoute
        {
            Name = "recipes-root",
            Path = "/recipes",
            Methods = new[] { "GET", "POST" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/recipes" } }
        },
        new MicroserviceRoute
        {
            Name = "recipes-all",
            Path = "/recipes/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/recipes/{**catch-all}" } }
        },
        new MicroserviceRoute
        {
            Name = "ingredients-root",
            Path = "/ingredients",
            Methods = new[] { "GET", "POST" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/ingredients" } }
        },
        new MicroserviceRoute
        {
            Name = "ingredients-all",
            Path = "/ingredients/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/ingredients/{**catch-all}" } }
        }
    };
}
