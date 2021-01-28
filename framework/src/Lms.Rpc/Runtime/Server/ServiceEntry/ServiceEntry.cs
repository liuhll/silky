using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.MethodExecutor;
using Lms.Rpc.Routing;
using Lms.Rpc.Runtime.Server.ServiceEntry.Descriptor;
using Lms.Rpc.Runtime.Server.ServiceEntry.Parameter;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public class ServiceEntry
    {
        private readonly ObjectMethodExecutor _methodExecutor;
        private readonly Type _serviceType;
        public ServiceEntry(IRouter router, ServiceDescriptor serviceDescriptor, Type serviceType,
            MethodInfo methodInfo, IReadOnlyList<ParameterDescriptor> parameterDescriptors, bool isLocal)
        {
            Router = router;
            ServiceDescriptor = serviceDescriptor;
            ParameterDescriptors = parameterDescriptors;
            IsLocal = isLocal;
            _serviceType = serviceType;
            var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(methodInfo);
            _methodExecutor =
                ObjectMethodExecutor.Create(methodInfo, serviceType.GetTypeInfo(), parameterDefaultValues);
            Executor = CreateExecutor();
        }

        public Func<string, IDictionary<ParameterFrom, object>, Task<object>> Executor { get; }

        public bool IsLocal { get; }

        public IRouter Router { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }

        private Func<string, IDictionary<ParameterFrom, object>, Task<object>> CreateExecutor() =>
            (key, parameters) => Task.Factory.StartNew(() =>
            {
                object instance = EngineContext.Current.Resolve(_serviceType);
                var list = new List<object>();
                foreach (var parameter in ParameterDescriptors)
                {
                    switch (parameter.From)
                    {
                        case ParameterFrom.Body:
                            break;
                        case ParameterFrom.Form:
                            break;
                        case ParameterFrom.Header:
                            break;
                        case ParameterFrom.Path:
                            break;
                        case ParameterFrom.Query:
                            break;
                    }
                }
                
                return _methodExecutor.ExecuteAsync(instance,list.ToArray()).GetAwaiter().GetResult();
            });

        public ServiceDescriptor ServiceDescriptor { get; }
    }
}