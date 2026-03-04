using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class PlansConfig : MicroserviceConfig
{
    public override string Name => "plans";
    public override string ClusterId => "plans";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PLANS_URL") ?? "http://localhost:8081";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "ingredients-all",
            Path = "/api/ingredients/{**catch-all}",
            Methods = new[] { "GET", "POST" , "PUT", "DELETE"},
            //AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/ingredients/{**catch-all}" } }
        },
        new MicroserviceRoute
        {
            Name = "recipes-all",
            Path = "/api/recipes/{**catch-all}",
            Methods = new[] { "GET", "POST" , "PUT", "DELETE" },
            //AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/recipes/{**catch-all}" } }
        },
        new MicroserviceRoute
        {
            Name = "meal-plans-all",
            Path = "/api/meal-plans/{**catch-all}",
            Methods = new[] { "GET", "POST" , "PUT", "DELETE" },
            //AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/meal-plans/{**catch-all}" } }
        }
    };
}
