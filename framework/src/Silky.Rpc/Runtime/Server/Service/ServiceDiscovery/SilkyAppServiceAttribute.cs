using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class SilkyAppServiceAttribute : MetadataAttribute
    {
        public SilkyAppServiceAttribute() : base(ServiceConstant.IsSilkyService, true)
        {
        }
    }
}