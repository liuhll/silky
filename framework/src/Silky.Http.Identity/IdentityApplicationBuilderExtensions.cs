namespace Microsoft.AspNetCore.Builder
{
    public static class IdentityApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSilkyIdentity(this IApplicationBuilder application)
        {
            application.UseAuthentication();
            application.UseAuthorization();
            return application;
        }
    }
}