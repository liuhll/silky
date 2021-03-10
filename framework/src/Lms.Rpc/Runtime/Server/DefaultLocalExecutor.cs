using System.Linq;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Convertible;

namespace Lms.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        public Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            return Task.Factory.StartNew<object>(() =>
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
                    return serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray()).GetAwaiter()
                        .GetResult();
                }

                return serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
            });

        }
    }
}