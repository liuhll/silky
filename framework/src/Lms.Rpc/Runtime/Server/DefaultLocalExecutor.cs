using System.Linq;
using Lms.Core;
using Lms.Core.Convertible;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;

namespace Lms.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        public object Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            object instance = null;
            if (!serviceKey.IsNullOrEmpty())
            {
                if (!EngineContext.Current.IsRegisteredWithName(serviceKey, serviceEntry.ServiceType))
                {
                    throw new UnServiceKeyImplementationException(
                        $"系统中没有存在serviceKey为{serviceKey}的{serviceEntry.ServiceType.FullName}接口的实现类");
                }

                instance = EngineContext.Current.ResolveNamed(serviceKey, serviceEntry.ServiceType);
            }
            else
            {
                instance = EngineContext.Current.Resolve(serviceEntry.ServiceType);
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && parameters[i].GetType() != serviceEntry.ParameterDescriptors[i].Type)
                {
                    var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                    parameters[i] = typeConvertibleService.Convert(parameters[i], serviceEntry.ParameterDescriptors[i].Type);
                }
            }

            if (serviceEntry.IsAsyncMethod)
            {
                return serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray()).GetAwaiter().GetResult();
            }

            return serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());

        }
    }
}