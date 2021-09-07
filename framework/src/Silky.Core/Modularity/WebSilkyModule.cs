using Microsoft.AspNetCore.Builder;

namespace Silky.Core.Modularity
{
    public abstract class WebSilkyModule : SilkyModule
    {
        public virtual void Configure(IApplicationBuilder application)
        {
            
        }
    }
}