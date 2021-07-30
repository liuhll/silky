using System.Linq;
using System.Threading.Tasks;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Runtime.Filters;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultLocalExecutor : ILocalExecutor
    {
        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null,
            MethodType methodType = MethodType.Try)
        {
            var instance = EngineContext.Current.ResolveServiceEntryInstance(serviceKey, serviceEntry.ServiceType);
            parameters = serviceEntry.ConvertParameters(parameters);

            var filters = EngineContext.Current.ResolveAll<IServerFilter>().OrderBy(p => p.Order).ToArray();
            var rpcActionExcutingContext = new ServiceEntryExecutingContext()
            {
                ServiceEntry = serviceEntry,
                Parameters = parameters,
                ServiceKey = serviceKey
            };

            foreach (var filter in filters)
            {
                filter.OnActionExecuting(rpcActionExcutingContext);
            }

            object result;
            
            if (serviceEntry.IsAsyncMethod)
            {
                switch (methodType)
                {
                    case MethodType.Try:
                        result = await serviceEntry.MethodExecutor.ExecuteAsync(instance, parameters.ToArray());
                        break;
                    case MethodType.Confirm:
                    case MethodType.Cancel:
                        var tccExcutorInfo = serviceEntry.GetTccExcutorInfo(instance, methodType);
                        if (!tccExcutorInfo.Item2)
                        {
                            throw new SilkyException(
                                $"The specified {methodType} method does not exist in the service instance",
                                StatusCode.NotExistMethod);
                        }
            
                        result = await tccExcutorInfo.Item1.ExecuteAsync(instance, parameters.ToArray());
                        break;
                    default:
                        throw new SilkyException($"MethodType specified error", StatusCode.NotExistMethod);
                }
            }
            else
            {
                switch (methodType)
                {
                    case MethodType.Try:
                        result = serviceEntry.MethodExecutor.Execute(instance, parameters.ToArray());
                        break;
                    case MethodType.Confirm:
                    case MethodType.Cancel:
                        var tccExcutorInfo = serviceEntry.GetTccExcutorInfo(instance, methodType);
                        if (!tccExcutorInfo.Item2)
                        {
                            throw new SilkyException(
                                $"The specified {methodType} method does not exist in the service instance",
                                StatusCode.NotExistMethod);
                        }
                        result = tccExcutorInfo.Item1.Execute(instance, parameters.ToArray());
                        break;
                    default:
                        throw new SilkyException($"MethodType specified error", StatusCode.NotExistMethod);
                }
            }

            var rpcActionExecutedContext = new ServiceEntryExecutedContext()
            {
                Result = result
            };
            foreach (var filter in filters)
            {
                filter.OnActionExecuted(rpcActionExecutedContext);
            }

            if (rpcActionExecutedContext.Exception != null)
            {
                throw rpcActionExecutedContext.Exception;
            }

            return rpcActionExecutedContext.Result;
        }
    }
}