using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Rpc.Runtime.Client;

namespace ITestApplication.Test.FallBack
{
    public class UpdatePartFallBack : IFallbackInvoker<string>
    {
        public async Task<string> Invoke(IDictionary<string, object> parameters)
        {
            return "UpdatePartFallBack";
        }
    }
}