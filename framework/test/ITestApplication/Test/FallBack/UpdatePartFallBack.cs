using System.Collections.Generic;
using System.Threading.Tasks;
using Lms.Rpc.Runtime.Support;

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