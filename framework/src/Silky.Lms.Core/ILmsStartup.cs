using Microsoft.AspNetCore.Builder;

namespace Silky.Lms.Core
{
    public interface ILmsStartup : IConfigureService
    {
        void Configure(IApplicationBuilder application);
    }
}