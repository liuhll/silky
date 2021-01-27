namespace Lms.Core.Exceptions
{
    public enum StatusCode
    {
        Success = 200,
        
        PlatformError = 500,
        
        RouteParseError = 502,
        
        RouteMatchError = 503,

        DataAccessError = 504,
    }
}