using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lms.Core;
using Lms.Core.Convertible;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;
using Lms.Core.MethodExecutor;
using Lms.Rpc.Configuration;
using Lms.Rpc.Routing;
using Lms.Rpc.Routing.Template;
using Lms.Rpc.Runtime.Client;
using Lms.Rpc.Runtime.Server.Descriptor;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Transport.CachingIntercept;

namespace Lms.Rpc.Runtime.Server
{
    public class ServiceEntry
    {
        private readonly ObjectMethodExecutor _methodExecutor;
        private readonly Type _serviceType;

        public bool FailoverCountIsDefaultValue { get; private set; }

        public bool MultipleServiceKey { get; private set; }

        public string Id => ServiceDescriptor.Id;

        public Type ServiceType => _serviceType;

        public ObjectMethodExecutor MethodExecutor => _methodExecutor;

        public ServiceEntry(IRouter router,
            ServiceDescriptor serviceDescriptor,
            Type serviceType,
            MethodInfo methodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            bool multipleServiceKey,
            bool isLocal,
            GovernanceOptions governanceOptions)
        {
            Router = router;
            ServiceDescriptor = serviceDescriptor;
            ParameterDescriptors = parameterDescriptors;
            IsLocal = isLocal;
            MultipleServiceKey = multipleServiceKey;
            _serviceType = serviceType;
            GroupName = serviceType.FullName;
            MethodInfo = methodInfo;
            CustomAttributes = MethodInfo.GetCustomAttributes(true);
            (IsAsyncMethod, ReturnType) = MethodInfo.ReturnTypeInfo();
            GovernanceOptions = new ServiceEntryGovernance();
            var governanceProvider = CustomAttributes.OfType<IGovernanceProvider>().FirstOrDefault();
            if (governanceProvider != null)
            {
                ReConfiguration(governanceProvider);
            }
            else
            {
                ReConfiguration(governanceOptions);
            }

            var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(methodInfo);
            _methodExecutor =
                ObjectMethodExecutor.Create(methodInfo, serviceType.GetTypeInfo(), parameterDefaultValues);

            Executor = CreateExecutor();
            CreateDefaultSupportedRequestMediaTypes();
            CreateDefaultSupportedResponseMediaTypes();
        }

        private void ReConfiguration(IGovernanceProvider governanceProvider)
        {
            if (governanceProvider != null)
            {
                GovernanceOptions.CacheEnabled = governanceProvider.CacheEnabled;
                GovernanceOptions.ExecutionTimeout = governanceProvider.ExecutionTimeout;
                GovernanceOptions.FuseProtection = governanceProvider.FuseProtection;
                GovernanceOptions.MaxConcurrent = governanceProvider.MaxConcurrent;
                GovernanceOptions.ShuntStrategy = governanceProvider.ShuntStrategy;
                GovernanceOptions.FuseSleepDuration = governanceProvider.FuseSleepDuration;
                GovernanceOptions.FailoverCount = governanceProvider.FailoverCount;
                FailoverCountIsDefaultValue = governanceProvider.FailoverCount == 0;
            }

            var governanceAttribute = governanceProvider as GovernanceAttribute;
            if (governanceAttribute?.FallBackType != null)
            {
                Type fallBackType;
                if (ReturnType == typeof(void))
                {
                    fallBackType = EngineContext.Current.TypeFinder.FindClassesOfType<IFallbackInvoker>()
                        .FirstOrDefault(p => p == governanceAttribute.FallBackType);
                    if (fallBackType == null)
                    {
                        throw new LmsException($"未能找到{governanceAttribute.FallBackType.FullName}的实现类");
                    }
                }
                else
                {
                    fallBackType = typeof(IFallbackInvoker<>);
                    fallBackType = fallBackType.MakeGenericType(ReturnType);
                    if (!EngineContext.Current.TypeFinder.FindClassesOfType(fallBackType)
                        .Any(p => p == governanceAttribute.FallBackType))
                    {
                        throw new LmsException($"未能找到{governanceAttribute.FallBackType.FullName}的实现类");
                    }
                }

                var invokeMethod = fallBackType.GetMethods().First(p => p.Name == "Invoke");

                var fallbackMethodExcutor = ObjectMethodExecutor.Create(invokeMethod, fallBackType.GetTypeInfo(),
                    ParameterDefaultValues.GetParameterDefaultValues(invokeMethod));
                FallBackExecutor = CreateFallBackExecutor(fallbackMethodExcutor, fallBackType);
            }
        }

        private Func<object[], Task<object>> CreateFallBackExecutor(
            ObjectMethodExecutor fallbackMethodExcutor, Type fallBackType)
        {
            return async parameters =>
            {
                var instance = EngineContext.Current.Resolve(fallBackType);
                return fallbackMethodExcutor.ExecuteAsync(instance, parameters).GetAwaiter().GetResult();
            };
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

        public Func<string, object[], Task<object>> Executor { get; }

        public IList<string> SupportedRequestMediaTypes { get; } = new List<string>();

        public IList<string> SupportedResponseMediaTypes { get; } = new List<string>();

        public bool IsLocal { get; }

        public string GroupName { get; }

        public IRouter Router { get; }

        public MethodInfo MethodInfo { get; }

        public bool IsAsyncMethod { get; }

        public Type ReturnType { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }

        public IReadOnlyCollection<object> CustomAttributes { get; }

        public ServiceEntryGovernance GovernanceOptions { get; }

        [CanBeNull] public Func<object[], Task<object>> FallBackExecutor { get; private set; }

        private Func<string, object[], Task<object>> CreateExecutor() =>
            (key, parameters) =>
            {
                if (IsLocal)
                {
                    var localServiceExecutor = EngineContext.Current.Resolve<ILocalExecutor>();
                    return localServiceExecutor.Execute(this, parameters, key);
                }

                var remoteServiceExecutor = EngineContext.Current.Resolve<IRemoteServiceExecutor>();
                return remoteServiceExecutor.Execute(this, parameters, key);
            };

        public ServiceDescriptor ServiceDescriptor { get; }


        public ICachingInterceptProvider GetCachingInterceptProvider =>
            CustomAttributes.OfType<IGetCachingInterceptProvider>()
                .FirstOrDefault();

        public ICachingInterceptProvider UpdateCachingInterceptProvider =>
            CustomAttributes.OfType<IUpdateCachingInterceptProvider>()
                .FirstOrDefault();

        public IReadOnlyCollection<IRemoveCachingInterceptProvider> RemoveCachingInterceptProviders =>
            CustomAttributes.OfType<IRemoveCachingInterceptProvider>()
                .ToImmutableList();

        public object[] ResolveParameters(IDictionary<ParameterFrom, object> parameters)
        {
            var list = new List<object>();
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameterDescriptor in ParameterDescriptors)
            {
                #region 获取参数

                var parameter = parameterDescriptor.From.DefaultValue();
                if (parameters.ContainsKey(parameterDescriptor.From))
                {
                    parameter = parameters[parameterDescriptor.From];
                }

                switch (parameterDescriptor.From)
                {
                    case ParameterFrom.Body:
                        list.Add(parameter);
                        break;
                    case ParameterFrom.Form:
                        if (parameterDescriptor.IsSample)
                        {
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameter);
                        }

                        break;
                    case ParameterFrom.Header:
                        if (parameterDescriptor.IsSample)
                        {
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameter);
                        }

                        break;
                    case ParameterFrom.Path:
                        if (parameterDescriptor.IsSample)
                        {
                            var pathVal =
                                (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                                    typeof(IDictionary<string, object>));
                            var parameterName = TemplateSegmentHelper.GetVariableName(parameterDescriptor.Name);
                            if (!pathVal.ContainsKey(parameterName))
                            {
                                throw new LmsException("path参数不允许为空,请确认您传递的参数是否正确");
                            }

                            var parameterVal = pathVal[parameterName];
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
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameter);
                        }

                        break;
                }

                #endregion
            }

            return list.ToArray();
        }

        private void SetSampleParameterValue(ITypeConvertibleService typeConvertibleService, object parameter,
            ParameterDescriptor parameterDescriptor, List<object> list)
        {
            var dict =
                (IDictionary<string, object>) typeConvertibleService.Convert(parameter,
                    typeof(IDictionary<string, object>));
            var parameterVal = parameterDescriptor.ParameterInfo.GetDefaultValue();
            if (dict.ContainsKey(parameterDescriptor.Name))
            {
                parameterVal = dict[parameterDescriptor.Name];
            }

            list.Add(parameterVal);
        }
    }
}