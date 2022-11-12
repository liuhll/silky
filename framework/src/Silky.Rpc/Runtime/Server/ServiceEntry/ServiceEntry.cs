using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.MethodExecutor;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Security;

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

        public ICollection<CachingInterceptorDescriptor> CachingInterceptorDescriptors =>
            ServiceEntryDescriptor.CachingInterceptorDescriptors;

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
            AuthorizeData = CreateAuthorizeData();

            ClientFilters = CreateClientFilters();
            ServerFilters = CreateServerFilters();
            CreateFallBackExecutor();
            CreateDefaultSupportedRequestMediaTypes();
            CreateDefaultSupportedResponseMediaTypes();
            CreateCachingInterceptorDescriptors();
        }

        private void CreateCachingInterceptorDescriptors()
        {
            var cachingInterceptorDescriptors = new List<CachingInterceptorDescriptor>();
            var cachingInterceptorProviders = this.GetAllCachingInterceptProviders();
            foreach (var cachingInterceptorProvider in cachingInterceptorProviders)
            {
                var cachingInterceptorDescriptor = new CachingInterceptorDescriptor()
                {
                    KeyTemplate = cachingInterceptorProvider.KeyTemplate,
                    OnlyCurrentUserData = cachingInterceptorProvider.OnlyCurrentUserData,
                    IgnoreMultiTenancy = cachingInterceptorProvider.IgnoreMultiTenancy,
                    CachingMethod = cachingInterceptorProvider.CachingMethod,
                    CacheName = this.GetCacheName(),
                    IsRemoveMatchKeyProvider = cachingInterceptorProvider is IRemoveMatchKeyCachingInterceptProvider,
                };
                if (cachingInterceptorProvider is IRemoveCachingInterceptProvider removeCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.CacheName = removeCachingInterceptProvider.CacheName;
                }
                
                if (cachingInterceptorProvider is IRemoveMatchKeyCachingInterceptProvider removeMatchKeyCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.CacheName = removeMatchKeyCachingInterceptProvider.CacheName;
                }
                
                if (cachingInterceptorProvider is IUpdateCachingInterceptProvider updateCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.IgnoreWhenCacheKeyNull = updateCachingInterceptProvider.IgnoreWhenCacheKeyNull;
                }

                foreach (var parameterDescriptor in ParameterDescriptors)
                {
                    foreach (var cacheKey in parameterDescriptor.CacheKeys)
                    {
                        cachingInterceptorDescriptor
                            .CacheKeyProviderDescriptors
                            .Add(new CacheKeyProviderDescriptor()
                            {
                                PropName = cacheKey.PropName,
                                Index = cacheKey.Index,
                                CacheKeyType = cacheKey.CacheKeyType,
                                ParameterIndex = parameterDescriptor.Index,
                                From = parameterDescriptor.From,
                                IsSampleOrNullableType = parameterDescriptor.IsSampleOrNullableType,
                            });
                    }
                }

                cachingInterceptorProvider.CachingInterceptorDescriptor = cachingInterceptorDescriptor;
                cachingInterceptorDescriptors.Add(cachingInterceptorDescriptor);
            }

            ServiceEntryDescriptor.CachingInterceptorDescriptors = cachingInterceptorDescriptors;
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
            foreach (var item in authorizeData)
            {
                ServiceEntryDescriptor.AuthorizeData.Add(new AuthorizeDescriptor()
                {
                    Policy = item.Policy,
                    AuthenticationSchemes = item.AuthenticationSchemes,
                    Roles = item.Roles
                });
            }

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
                ServiceEntryDescriptor.RouteOrder = Router.RouteTemplate.Order;
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
            if (!this.NeedHttpProtocolSupport() && (ReturnType != null || ReturnType == typeof(void)))
            {
                SupportedResponseMediaTypes.Add("application/json");
                SupportedResponseMediaTypes.Add("text/json");
            }
        }

        private void CreateDefaultSupportedRequestMediaTypes()
        {
            if (ParameterDescriptors.Any(p => p.From == ParameterFrom.Form) || this.NeedHttpProtocolSupport())
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


        internal void UpdateGovernance(GovernanceOptions options)
        {
            ReConfiguration(options);
        }
    }
}