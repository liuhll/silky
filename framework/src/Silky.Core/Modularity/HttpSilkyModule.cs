using Microsoft.AspNetCore.Builder;

namespace Silky.Core.Modularity
{
    public abstract class HttpSilkyModule : SilkyModule
    {
        public virtual void Configure(IApplicationBuilder application)
        {
        }
    }
}