using System.Linq;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class DefaultServiceEntryLocate : IServiceEntryLocate
    {
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServiceEntryLocate(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }


        public ServiceEntry GetServiceEntryByApi(string path, HttpMethod httpMethod)
        {
            return _serviceEntryManager.GetAllEntries()
                .FirstOrDefault(p => p.Router.IsMatch(path, httpMethod));
        }

        public ServiceEntry GetServiceEntryById(string id)
        {
            throw new System.NotImplementedException();
        }

        public ServiceEntry GetLocalServiceEntryById(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}