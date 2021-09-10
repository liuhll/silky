using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Configuration;
using Silky.Rpc.MiniProfiler;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Template;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server.Parameter;
using Silky.Rpc.Transport.CachingIntercept;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntry
    {
        private readonly ObjectMethodExecutor _methodExecutor;

        private readonly Type _serviceType;
        public bool FailoverCountIsDefaultValue { get; private set; }

        public bool MultipleServiceKey { get; private set; }

        public string Id => ServiceEntryDescriptor.Id;

        public string ServiceId => ServiceEntryDescriptor.ServiceId;
        
        public string Application => ServiceEntryDescriptor.Application;

        public Type ServiceType => _serviceType;

        public ObjectMethodExecutor MethodExecutor => _methodExecutor;

        internal ServiceEntry(IRouter router,
            ServiceEntryDescriptor serviceEntryDescriptor,
            Type serviceType,
            MethodInfo methodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            IRouteTemplateProvider routeTemplateProvider,
            bool isLocal,
            GovernanceOptions governanceOptions)
        {
            Router = router;
            ServiceEntryDescriptor = serviceEntryDescriptor;
            ParameterDescriptors = parameterDescriptors;
            _serviceType = serviceType;
            IsLocal = isLocal;

            MultipleServiceKey = routeTemplateProvider.MultipleServiceKey;
            MethodInfo = methodInfo;
            CustomAttributes = MethodInfo.GetCustomAttributes(true);
            (IsAsyncMethod, ReturnType) = MethodInfo.ReturnTypeInfo();
            GovernanceOptions = new ServiceEntryGovernance();

            var serviceDescriptionAttribute = methodInfo.DeclaringType?.GetCustomAttributes()
                .OfType<ServiceDescriptionAttribute>().FirstOrDefault();
            if (serviceDescriptionAttribute != null)
            {
                GroupName = serviceDescriptionAttribute.Name;
            }
            else
            {
                GroupName = !routeTemplateProvider.ServiceName.IsNullOrEmpty()
                    ? routeTemplateProvider.ServiceName
                    : _serviceType.Name.TrimStart('I');
            }

            var governanceProvider = CustomAttributes.OfType<IGovernanceProvider>().FirstOrDefault();
            if (governanceProvider != null)
            {
                ReConfiguration(governanceProvider);
            }
            else
            {
                ReConfiguration(governanceOptions);
            }

            _methodExecutor = methodInfo.CreateExecutor(serviceType);
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
                        throw new SilkyException(
                            $"Could not find the implementation class of {governanceAttribute.FallBackType.FullName}");
                    }
                }
                else
                {
                    fallBackType = typeof(IFallbackInvoker<>);
                    fallBackType = fallBackType.MakeGenericType(ReturnType);
                    if (!EngineContext.Current.TypeFinder.FindClassesOfType(fallBackType)
                        .Any(p => p == governanceAttribute.FallBackType))
                    {
                        throw new SilkyException(
                            $"Could not find the implementation class of {governanceAttribute.FallBackType.FullName}");
                    }
                }

                var invokeMethod = fallBackType.GetMethods().First(p => p.Name == "Invoke");

                var fallbackMethodExcutor = ObjectMethodExecutor.Create(invokeMethod, fallBackType.GetTypeInfo(),
                    ParameterDefaultValues.GetParameterDefaultValues(invokeMethod));
                FallBackExecutor = CreateFallBackExecutor(fallbackMethodExcutor, fallBackType);
            }

            GovernanceOptions.ProhibitExtranet = governanceAttribute?.ProhibitExtranet ?? false;

            var allowAnonymous = CustomAttributes.OfType<IAllowAnonymous>().FirstOrDefault() ?? MethodInfo.DeclaringType
                .GetCustomAttributes().OfType<IAllowAnonymous>().FirstOrDefault();

            GovernanceOptions.IsAllowAnonymous = allowAnonymous != null;
        }

        private Func<object[], Task<object>> CreateFallBackExecutor(
            ObjectMethodExecutor fallbackMethodExcutor, Type fallBackType)
        {
            return async parameters =>
            {
                try
                {
                    MiniProfilerPrinter.Print(MiniProfileConstant.FallBackExecutor.Name,
                        MiniProfileConstant.FallBackExecutor.State.Begin,
                        "Start execution failure callback method");
                    var instance = EngineContext.Current.Resolve(fallBackType);
                    var result = fallbackMethodExcutor.ExecuteAsync(instance, parameters).GetAwaiter().GetResult();
                    MiniProfilerPrinter.Print(MiniProfileConstant.FallBackExecutor.Name,
                        MiniProfileConstant.FallBackExecutor.State.Success,
                        "Failed callback executed successfully");
                    return result;
                }
                catch (Exception e)
                {
                    MiniProfilerPrinter.Print(MiniProfileConstant.FallBackExecutor.Name,
                        MiniProfileConstant.FallBackExecutor.State.Fail,
                        $"Failure callback execution failed, reason:{e.Message}", true);
                    throw;
                }
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
                RpcContext.Context.SetAttachment(AttachmentKeys.ServiceMethodName,
                    $"{MethodInfo.DeclaringType?.FullName}.{MethodInfo.Name}");

                if (IsLocal)
                {
                    var localServiceExecutor = EngineContext.Current.Resolve<ILocalExecutor>();
                    return localServiceExecutor.Execute(this, parameters, key);
                }

                var remoteServiceExecutor = EngineContext.Current.Resolve<IRemoteServiceExecutor>();
                return remoteServiceExecutor.Execute(this, parameters, key);
            };

        public ServiceEntryDescriptor ServiceEntryDescriptor { get; }


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
                                (IDictionary<string, object>)typeConvertibleService.Convert(parameter,
                                    typeof(IDictionary<string, object>));
                            var parameterName = TemplateSegmentHelper.GetVariableName(parameterDescriptor.Name);
                            if (!pathVal.ContainsKey(parameterName))
                            {
                                throw new SilkyException(
                                    "The path parameter is not allowed to be empty, please confirm whether the parameter you passed is correct");
                            }

                            var parameterVal = pathVal[parameterName];
                            list.Add(typeConvertibleService.Convert(parameterVal, parameterDescriptor.Type));
                        }
                        else
                        {
                            throw new SilkyException(
                                "Complex data types do not support access through routing templates");
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
                (IDictionary<string, object>)typeConvertibleService.Convert(parameter,
                    typeof(IDictionary<string, object>));
            var parameterVal = parameterDescriptor.ParameterInfo.GetDefaultValue();
            if (dict.ContainsKey(parameterDescriptor.Name))
            {
                parameterVal = dict[parameterDescriptor.Name];
            }

            list.Add(parameterVal);
        }

        internal void UpdateGovernance(GovernanceOptions options)
        {
            ReConfiguration(options);
        }
    }
}