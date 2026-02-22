using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class PatientConfig : MicroserviceConfig
{
    public override string Name => "patients";
    public override string ClusterId => "patients";
    public override string BaseUrl => Environment.GetEnvironmentVariable("PATIENTS_URL")
        ?? "http://localhost:8080";

    public override List<MicroserviceRoute> GetRoutes() => new()
    {
        new MicroserviceRoute
        {
            Name = "patients-root",
            Path = "/patients",
            Methods = new[] { "GET", "POST" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/patient" } }
        },
        new MicroserviceRoute
        {
            Name = "patients-all",
            Path = "/patients/{**catch-all}",
            Methods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" },
            AuthorizationPolicy = AuthPolicies.Authenticated,
            CustomTransforms = new() { { "PathPattern", "/api/patient/{**catch-all}" } }
        }
    };
}
