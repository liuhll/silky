using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lms.Core;
using Lms.Core.Convertible;
using Lms.Core.Exceptions;
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
            MethodInfo = methodInfo;
            GroupName = serviceType.FullName;
            CustomAttributes = serviceType.GetCustomAttributes(true);
            var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(methodInfo);
            _methodExecutor =
                ObjectMethodExecutor.Create(methodInfo, serviceType.GetTypeInfo(), parameterDefaultValues);
            Executor = CreateExecutor();
            CreateDefaultSupportedRequestMediaTypes();
        }

        private void CreateDefaultSupportedRequestMediaTypes()
        {
            if (ParameterDescriptors.Any(p=> p.From == ParameterFrom.Form))
            {
                SupportedRequestMediaTypes.Add("multipart/form-data");
            }
            else
            {
                SupportedRequestMediaTypes.Add("Application/json");
            }
        }

        public Func<string, IDictionary<ParameterFrom, object>, Task<object>> Executor { get; }

        public IList<string> SupportedRequestMediaTypes { get; } = new List<string>();

        public bool IsLocal { get; }

        public string GroupName { get; }
        
        public IRouter Router { get; }

        public MethodInfo MethodInfo { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }

        public IReadOnlyCollection<object> CustomAttributes { get; }

        private Func<string, IDictionary<ParameterFrom, object>, Task<object>> CreateExecutor() =>
            (key, parameters) => Task.Factory.StartNew(() =>
            {
                object instance = EngineContext.Current.Resolve(_serviceType);
                var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();

                var list = new List<object>();
                foreach (var parameterDescriptor in ParameterDescriptors)
                {
                    #region 获取参数

                    var parameter = parameters[parameterDescriptor.From];
                    switch (parameterDescriptor.From)
                    {
                        case ParameterFrom.Body:
                            list.Add(typeConvertibleService.Convert(parameter, parameterDescriptor.Type));
                            break;
                        case ParameterFrom.Form:
                            if (parameterDescriptor.IsSample)
                            {
                                var formVal =
                                    (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                                        typeof(IDictionary<string, object>));
                                var parameterVal = formVal[parameterDescriptor.Name];
                                list.Add(parameterVal);
                            }
                            else
                            {
                                list.Add(typeConvertibleService.Convert(parameter, parameterDescriptor.Type));
                            }

                            break;
                        case ParameterFrom.Header:
                            if (parameterDescriptor.IsSample)
                            {
                                var formVal =
                                    (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                                        typeof(IDictionary<string, object>));
                                var parameterVal = formVal[parameterDescriptor.Name];
                                list.Add(parameterVal);
                            }
                            else
                            {
                                list.Add(typeConvertibleService.Convert(parameter, parameterDescriptor.Type));
                            }

                            break;
                        case ParameterFrom.Path:
                            if (parameterDescriptor.IsSample)
                            {
                                var formVal =
                                    (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                                        typeof(IDictionary<string, object>));
                                var parameterVal = formVal[parameterDescriptor.Name];
                                list.Add(parameterVal);
                            }
                            else
                            {
                                throw new LmsException("复杂数据类型不支持通过路由模板进行获取");
                            }

                            break;
                        case ParameterFrom.Query:
                            if (parameterDescriptor.IsSample)
                            {
                                var formVal =
                                    (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                                        typeof(IDictionary<string, object>));
                                var parameterVal = formVal[parameterDescriptor.Name];
                                list.Add(parameterVal);
                            }
                            else
                            {
                                list.Add(typeConvertibleService.Convert(parameter, parameterDescriptor.Type));
                            }

                            break;
                    }

                    #endregion
                }

                return _methodExecutor.ExecuteAsync(instance, list.ToArray()).GetAwaiter().GetResult();
            });

        public ServiceDescriptor ServiceDescriptor { get; }
    }
}