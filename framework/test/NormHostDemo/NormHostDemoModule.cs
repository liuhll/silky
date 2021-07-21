using Microsoft.Extensions.Hosting;
using Silky.Codec;
using Silky.Core.Modularity;

namespace NormHostDemo
{
    [DependsOn(typeof(MessagePackModule))]
    public class NormHostDemoModule : GeneralHostModule
    {
        
    }
}