using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface ISilkyStartup : IConfigureService
    {
        void Configure(IApplicationBuilder application);

        int Order { get; }
    }
}