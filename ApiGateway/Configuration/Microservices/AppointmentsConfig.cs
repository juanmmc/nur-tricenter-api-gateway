using ApiGateway.Security;

namespace ApiGateway.Configuration.Microservices;

public class AppointmentConfig : MicroserviceConfig {
  public override string Name => "appointments";
  public override string ClusterId => "appointments";
  public override string BaseUrl => Environment.GetEnvironmentVariable("APPOINTMENTS_URL")
      ?? "http://146.190.116.159:9004";
  public override bool UseDiscovery => true;
  public override string ConsulServiceName => "nur-tricenter-appointments";

  public override List<MicroserviceRoute> GetRoutes() => new()
  {
      new MicroserviceRoute
      {
          Name = "nutritionist-appointments",
          Path = "/nutritionist/appointments",
          Methods = new[] { "POST" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist/appointments" } }
      },
      new MicroserviceRoute
      {
          Name = "nutritionist-list",
          Path = "/nutritionist",
          Methods = new[] { "GET" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist" } }
      },
      new MicroserviceRoute
      {
          Name = "nutritionist-write",
          Path = "/nutritionist",
          Methods = new[] { "POST", "PUT", "DELETE" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist" } }
      },
      new MicroserviceRoute
      {
          Name = "nutritionist-by-id",
          Path = "/nutritionist/{id}",
          Methods = new[] { "GET" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist/{id}" } }
      },
      new MicroserviceRoute
      {
          Name = "nutritionist-by-id-write",
          Path = "/nutritionist/{id}",
          Methods = new[] { "PUT", "DELETE" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/nutritionist/{id}" } }
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
      },
      new MicroserviceRoute
      {
          Name = "appointment-cancel",
          Path = "/appointment/cancel",
          Methods = new[] { "PATCH" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/appointment/cancel" } }
      },
      new MicroserviceRoute
      {
          Name = "appointment-notattended",
          Path = "/appointment/notattended",
          Methods = new[] { "PATCH" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/appointment/notattended" } }
      },
      new MicroserviceRoute
      {
          Name = "appointment-by-id",
          Path = "/appointment/{id}",
          Methods = new[] { "GET" },
          AuthorizationPolicy = AuthPolicies.Authenticated,
          CustomTransforms = new() { { "PathPattern", "/api/appointment/{id}" } }
      }
  };
}