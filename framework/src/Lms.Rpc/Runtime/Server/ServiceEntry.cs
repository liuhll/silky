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
using Lms.Rpc.Routing.Template;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server.Descriptor;
using Lms.Rpc.Runtime.Server.Parameter;

namespace Lms.Rpc.Runtime.Server
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
            GroupName = serviceType.FullName;
            MethodInfo = methodInfo;
            CustomAttributes = serviceType.GetCustomAttributes(true);
            ReturnType = SetReturnType();
            var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(methodInfo);
            _methodExecutor =
                ObjectMethodExecutor.Create(methodInfo, serviceType.GetTypeInfo(), parameterDefaultValues);
            Executor = CreateExecutor();
            CreateDefaultSupportedRequestMediaTypes();
            CreateDefaultSupportedResponseMediaTypes();
        }

        private Type SetReturnType()
        {
            var returnType = MethodInfo.ReturnType;

            IsAsyncMethod = returnType == typeof(Task) || returnType.BaseType == typeof(Task);
            if (IsAsyncMethod)
            {
                return returnType.GenericTypeArguments.FirstOrDefault();
            }

            return returnType;
        }

        private void CreateDefaultSupportedResponseMediaTypes()
        {
            if (ReturnType != null || ReturnType == typeof(void))
            {
                SupportedResponseMediaTypes.Add("application/json");
                SupportedResponseMediaTypes.Add("text/json");
            }
        }

        private void CreateDefaultSupportedRequestMediaTypes()
        {
            if (ParameterDescriptors.Any(p => p.From == ParameterFrom.Form))
            {
                SupportedRequestMediaTypes.Add("multipart/form-data");
            }
            else
            {
                SupportedRequestMediaTypes.Add("application/json");
                SupportedRequestMediaTypes.Add("text/json");
            }
        }

        public Func<string, IList<object>, Task<object>> Executor { get; }

        public IList<string> SupportedRequestMediaTypes { get; } = new List<string>();

        public IList<string> SupportedResponseMediaTypes { get; } = new List<string>();

        public bool IsLocal { get; }

        public string GroupName { get; }

        public IRouter Router { get; }

        public MethodInfo MethodInfo { get; }

        public bool IsAsyncMethod { get; private set; }

        public Type ReturnType { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }

        public IReadOnlyCollection<object> CustomAttributes { get; }

        private Func<string, IList<object>, Task<object>> CreateExecutor() =>
            (key, parameters) => Task.Factory.StartNew(() =>
            {
                if (IsLocal)
                {
                    object instance = EngineContext.Current.Resolve(_serviceType);
                    if (IsAsyncMethod)
                    {
                        return _methodExecutor.ExecuteAsync(instance, parameters.ToArray()).GetAwaiter().GetResult();
                    }

                    return _methodExecutor.Execute(instance, parameters.ToArray());
                }
                var remoteServiceExecutor = EngineContext.Current.Resolve<IRemoteServiceExecutor>();
                return remoteServiceExecutor.Execute(this, parameters).GetAwaiter().GetResult();
            });

        public IList<object> ResolveParameters(IDictionary<ParameterFrom, object> parameters)
        {
            var list = new List<object>();
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
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
                            var parameterVal = formVal[TemplateSegmentHelper.GetVariableName(parameterDescriptor.Name)];
                            list.Add(typeConvertibleService.Convert(parameterVal, parameterDescriptor.Type));
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
                            list.Add(typeConvertibleService.Convert(parameterVal, parameterDescriptor.Type));
                        }
                        else
                        {
                            list.Add(typeConvertibleService.Convert(parameter, parameterDescriptor.Type));
                        }

                        break;
                }

                #endregion
            }

            return list;
        }

        public ServiceDescriptor ServiceDescriptor { get; }
    }
}