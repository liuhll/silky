namespace Silky.Http.Swagger.Configuration;

public class SwaggerLoginInfo
{

    public bool Enabled { get; set; }
    
    public string CheckUrl { get; set; }
    
    public string SubmitUrl { get; set; }

    public bool UseTenantName { get; set; }
}