using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Routing.Template;
using Silky.Rpc.Runtime.Client;

namespace Silky.Rpc.Runtime.Server
{
    public class ServiceEntry
    {
        private readonly ObjectMethodExecutor _methodExecutor;

        private readonly Type _serviceType;

        private readonly ServiceEntryDescriptor _serviceEntryDescriptor;

        public string Id => ServiceEntryDescriptor.Id;

        public string ServiceId => ServiceEntryDescriptor.ServiceId;

        public Type ServiceType => _serviceType;

        public ObjectMethodExecutor MethodExecutor => _methodExecutor;

        [CanBeNull] public ObjectMethodExecutor FallbackMethodExecutor { get; private set; }
        [CanBeNull] public IFallbackProvider FallbackProvider { get; private set; }

        internal ServiceEntry(IRouter router,
            ServiceEntryDescriptor serviceEntryDescriptor,
            Type serviceType,
            MethodInfo methodInfo,
            IReadOnlyList<ParameterDescriptor> parameterDescriptors,
            bool isLocal,
            GovernanceOptions governanceOptions)
        {
            Router = router;
            _serviceEntryDescriptor = serviceEntryDescriptor;
            ParameterDescriptors = parameterDescriptors;
            _serviceType = serviceType;
            IsLocal = isLocal;
            MethodInfo = methodInfo;
            CustomAttributes = MethodInfo.GetCustomAttributes(true);
            (IsAsyncMethod, ReturnType) = MethodInfo.ReturnTypeInfo();
            GovernanceOptions = new ServiceEntryGovernance(governanceOptions);

            var governanceProvider = CustomAttributes.OfType<IGovernanceProvider>().FirstOrDefault();
            ReConfiguration(governanceProvider);


            _methodExecutor = methodInfo.CreateExecutor(serviceType);
            Executor = CreateExecutor();
            if (EngineContext.Current.IsRegistered(typeof(IAuthorizationService)))
            {
                AuthorizeData = CreateAuthorizeData();
            }

            ClientFilters = CreateClientFilters();
            ServerFilters = CreateServerFilters();
            CreateFallBackExecutor();
            CreateDefaultSupportedRequestMediaTypes();
            CreateDefaultSupportedResponseMediaTypes();
        }

        private IReadOnlyCollection<IServerFilter> CreateServerFilters()
        {
            var serviceEntryServerFilters = CustomAttributes.OfType<IServerFilter>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);
            var serviceServerFilters = ServiceType.GetCustomAttributes().OfType<IServerFilter>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);
            var serverFilters = new List<IServerFilter>();
            serverFilters.AddRange(serviceEntryServerFilters);
            serverFilters.AddRange(serviceServerFilters);
            return serverFilters.ToArray();
        }

        private IReadOnlyCollection<IClientFilter> CreateClientFilters()
        {
            var serviceEntryClientFilters = CustomAttributes.OfType<IClientFilter>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);
            var serviceClientFilters = ServiceType.GetCustomAttributes().OfType<IClientFilter>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);
            var clientFilters = new List<IClientFilter>();
            clientFilters.AddRange(serviceEntryClientFilters);
            clientFilters.AddRange(serviceClientFilters);
            return clientFilters.ToArray();
        }

        private IReadOnlyCollection<IAuthorizeData> CreateAuthorizeData()
        {
            var authorizeData = new List<IAuthorizeData>();
            var serviceEntryAuthorizeData = CustomAttributes.OfType<IAuthorizeData>();
            authorizeData.AddRange(serviceEntryAuthorizeData);
            var serviceAuthorizeData = ServiceType.GetCustomAttributes().OfType<IAuthorizeData>();
            authorizeData.AddRange(serviceAuthorizeData);

            return authorizeData;
        }

        private void ReConfiguration(IGovernanceProvider governanceProvider)
        {
            if (governanceProvider != null)
            {
                GovernanceOptions.EnableCachingInterceptor = governanceProvider.EnableCachingInterceptor;
                GovernanceOptions.TimeoutMillSeconds = governanceProvider.TimeoutMillSeconds;
                GovernanceOptions.EnableCircuitBreaker = governanceProvider.EnableCircuitBreaker;
                GovernanceOptions.BreakerSeconds = governanceProvider.BreakerSeconds;
                GovernanceOptions.ExceptionsAllowedBeforeBreaking = governanceProvider.ExceptionsAllowedBeforeBreaking;
                GovernanceOptions.ShuntStrategy = governanceProvider.ShuntStrategy;
                GovernanceOptions.RetryTimes = governanceProvider.RetryTimes;

                GovernanceOptions.RetryIntervalMillSeconds = governanceProvider.RetryIntervalMillSeconds;
                var governanceAttribute = governanceProvider as GovernanceAttribute;
                GovernanceOptions.ProhibitExtranet = governanceAttribute?.ProhibitExtranet ?? false;
            }

            var allowAnonymous = CustomAttributes.OfType<IAllowAnonymous>().FirstOrDefault();

            if (allowAnonymous != null)
            {
                GovernanceOptions.IsAllowAnonymous = true;
            }
            else
            {
                var authorizeData = CustomAttributes.OfType<IAuthorizeData>();
                if (!authorizeData.Any())
                {
                    allowAnonymous = _serviceType.GetCustomAttributes().OfType<IAllowAnonymous>().FirstOrDefault();
                    if (allowAnonymous != null)
                    {
                        GovernanceOptions.IsAllowAnonymous = true;
                    }
                }
            }

            if (_serviceType.GetCustomAttributes().OfType<DashboardAppServiceAttribute>().Any())
            {
                if (EngineContext.Current.Configuration.GetValue<bool?>(ServiceEntryConstant.DashboardUseAuth) == false)
                {
                    GovernanceOptions.IsAllowAnonymous = true;
                }
            }

            if (_serviceType.GetCustomAttributes().OfType<ProhibitExtranetAttribute>().Any() ||
                CustomAttributes.OfType<ProhibitExtranetAttribute>().Any())
            {
                GovernanceOptions.ProhibitExtranet = true;
            }

            UpdateServiceEntryDescriptor(GovernanceOptions);
        }

        private void UpdateServiceEntryDescriptor(ServiceEntryGovernance serviceEntryGovernance)
        {
            ServiceEntryDescriptor.IsAllowAnonymous = serviceEntryGovernance.IsAllowAnonymous;
            ServiceEntryDescriptor.ProhibitExtranet = serviceEntryGovernance.ProhibitExtranet;
            ServiceEntryDescriptor.IsDistributeTransaction = this.IsTransactionServiceEntry();
            if (!serviceEntryGovernance.ProhibitExtranet)
            {
                ServiceEntryDescriptor.WebApi = Router.RoutePath;
                ServiceEntryDescriptor.HttpMethod = Router.HttpMethod;
            }

            ServiceEntryDescriptor.GovernanceOptions = serviceEntryGovernance;
        }

        private void CreateFallBackExecutor()
        {
            var fallbackProvider = CustomAttributes
                .OfType<IFallbackProvider>().FirstOrDefault();

            if (fallbackProvider != null)
            {
                if (!EngineContext.Current.IsRegistered(fallbackProvider.Type))
                {
                    return;
                }

                var compareMethodName = fallbackProvider.MethodName ?? MethodInfo.Name;
                var fallbackMethod = fallbackProvider.Type.GetCompareMethod(MethodInfo, compareMethodName);
                if (fallbackMethod == null)
                {
                    return;
                }

                var parameterDefaultValues = ParameterDefaultValues.GetParameterDefaultValues(fallbackMethod);
                FallbackMethodExecutor =
                    ObjectMethodExecutor.Create(fallbackMethod, fallbackProvider.Type.GetTypeInfo(),
                        parameterDefaultValues);

                FallbackProvider = fallbackProvider;
            }
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

        public IRouter Router { get; }

        public MethodInfo MethodInfo { get; }

        public bool IsAsyncMethod { get; }

        public Type ReturnType { get; }

        public IReadOnlyList<ParameterDescriptor> ParameterDescriptors { get; }

        public IReadOnlyCollection<object> CustomAttributes { get; }

        public IReadOnlyCollection<IClientFilter> ClientFilters { get; private set; }

        public IReadOnlyCollection<IServerFilter> ServerFilters { get; private set; }

        public IReadOnlyCollection<IAuthorizeData> AuthorizeData { get; }

        public ServiceEntryGovernance GovernanceOptions { get; }

        private Func<string, object[], Task<object>> CreateExecutor() =>
            (key, parameters) =>
            {
                RpcContext.Context.SetInvokeAttachment(AttachmentKeys.ServiceMethodName,
                    $"{MethodInfo.DeclaringType?.FullName}.{MethodInfo.Name}");

                if (IsLocal)
                {
                    var localServiceExecutor = EngineContext.Current.Resolve<ILocalExecutor>();
                    return localServiceExecutor.Execute(this, parameters, key);
                }

                var remoteServiceExecutor = EngineContext.Current.Resolve<IRemoteExecutor>();
                return remoteServiceExecutor.Execute(this, parameters, key);
            };

        public ServiceEntryDescriptor ServiceEntryDescriptor => _serviceEntryDescriptor;

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
                        list.Add(parameterDescriptor.GetActualParameter(parameter));
                        break;
                    case ParameterFrom.Form:
                        if (parameterDescriptor.IsSampleOrNullableType)
                        {
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameterDescriptor.GetActualParameter(parameter));
                        }

                        break;
                    case ParameterFrom.Header:
                        if (parameterDescriptor.IsSampleOrNullableType)
                        {
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameterDescriptor.GetActualParameter(parameter));
                        }

                        break;
                    case ParameterFrom.Path:
                        if (parameterDescriptor.IsSampleOrNullableType)
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
                            list.Add(parameterDescriptor.GetActualParameter(parameterVal));
                        }
                        else
                        {
                            throw new SilkyException(
                                "Complex data types do not support access through routing templates");
                        }

                        break;
                    case ParameterFrom.Query:
                        if (parameterDescriptor.IsSampleOrNullableType)
                        {
                            SetSampleParameterValue(typeConvertibleService, parameter, parameterDescriptor, list);
                        }
                        else
                        {
                            list.Add(parameterDescriptor.GetActualParameter(parameter));
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

            list.Add(parameterDescriptor.GetActualParameter(parameterVal));
        }

        internal void UpdateGovernance(GovernanceOptions options)
        {
            ReConfiguration(options);
        }
    }
}