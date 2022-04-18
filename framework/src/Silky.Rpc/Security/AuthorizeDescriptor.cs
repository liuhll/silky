namespace Silky.Rpc.Security;

public class AuthorizeDescriptor
{

    public string? Policy { get; set; }

    public string? Roles { get; set; }

    public string? AuthenticationSchemes { get; set; }
}