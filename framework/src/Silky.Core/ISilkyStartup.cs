using Microsoft.AspNetCore.Builder;

namespace Silky.Core
{
    public interface ISilkyStartup : IConfigureService
    {
        void Configure(IApplicationBuilder application);
    }
}