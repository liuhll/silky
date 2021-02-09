using Lms.Core.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server
{
    public interface IServiceEntryLocator : ISingletonDependency
    {
        ServiceEntry GetServiceEntryByApi(string path, HttpMethod httpMethod);

        ServiceEntry GetServiceEntryById(string id);

        ServiceEntry GetLocalServiceEntryById(string id);
    }
}