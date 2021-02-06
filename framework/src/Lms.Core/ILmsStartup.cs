using Microsoft.AspNetCore.Builder;

namespace Lms.Core
{
    public interface ILmsStartup : IConfigureService
    {
        void Configure(IApplicationBuilder application);
    }
}