using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Convertible;

namespace Lms.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            var instance = EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && parameters[i].GetType() != serviceEntry.ParameterDescriptors[i].Type)
                {
                    var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                    parameters[i] =
                        typeConvertibleService.Convert(parameters[i], serviceEntry.ParameterDescriptors[i].Type);
                }
            }

            if (serviceEntry.IsAsyncMethod)
            {
                return await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
            }

            return serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
        }
    }
}