using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class AppointmentConfig : MicroserviceConfig {
  public override string Name => "appointments";
  public override string ClusterId => "appointments";
  public override string BaseUrl => Environment.GetEnvironmentVariable("APPOINTMENTS_URL")
      ?? "http://localhost:9004";

  public override List<MicroserviceRoute> GetRoutes() => new()
  {
      new MicroserviceRoute
      {
          Name = "nutritionist-all",
          Path = "/nutritionist",
          Methods = new[] { "GET", "POST" , "PUT", "DELETE"},
          AuthorizationPolicy = AuthPolicies.AdminOnly,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist" } }
      },
      new MicroserviceRoute
      {
          Name = "appointment-schedule",
          Path = "/appointment/schedule",
          Methods = new[] { "POST" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/appointment/schedule" } }
      },
      new MicroserviceRoute
      {
          Name = "appointment-attend",
          Path = "/appointment/attend",
          Methods = new[] { "POST" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/appointment/attend" } }
      }
  };
}