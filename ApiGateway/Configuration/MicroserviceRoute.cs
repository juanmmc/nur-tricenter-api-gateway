namespace ApiGateway.Configuration;

public class MicroserviceRoute
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required string[] Methods { get; set; }
    public string? AuthorizationPolicy { get; set; }
    public Dictionary<string, string>? CustomTransforms { get; set; }
}