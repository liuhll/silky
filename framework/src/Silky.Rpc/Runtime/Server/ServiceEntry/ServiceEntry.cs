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
using Silky.Core.FilterMetadata;
using Silky.Core.MethodExecutor;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Configuration;
using Silky.Rpc.Endpoint.Selector;
using Silky.Rpc.Filters;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Security;
using AuthorizeAttribute = Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
using FilterDescriptor = Silky.Rpc.Filters.FilterDescriptor;

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
            IReadOnlyList<RpcParameter> parameters,
            bool isLocal,
            GovernanceOptions governanceOptions)
        {
            Router = router;
            _serviceEntryDescriptor = serviceEntryDescriptor;
            Parameters = parameters;
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
                var ignoreMultiTenancy = this.GetCacheIgnoreMultiTenancy(cachingInterceptorProvider);
                cachingInterceptorProvider.IgnoreMultiTenancy = ignoreMultiTenancy;
                var cachingInterceptorDescriptor = new CachingInterceptorDescriptor()
                {
                    KeyTemplate = cachingInterceptorProvider.KeyTemplate,
                    OnlyCurrentUserData = cachingInterceptorProvider.OnlyCurrentUserData,
                    IgnoreMultiTenancy = ignoreMultiTenancy,
                    CachingMethod = cachingInterceptorProvider.CachingMethod,
                    CacheName = this.GetCacheName(),
                    IsRemoveMatchKeyProvider = cachingInterceptorProvider is IRemoveMatchKeyCachingInterceptProvider,
                };
                if (cachingInterceptorProvider is IRemoveCachingInterceptProvider removeCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.CacheName = removeCachingInterceptProvider.CacheName;
                }

                if (cachingInterceptorProvider is IRemoveMatchKeyCachingInterceptProvider
                    removeMatchKeyCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.CacheName = removeMatchKeyCachingInterceptProvider.CacheName;
                }

                if (cachingInterceptorProvider is IUpdateCachingInterceptProvider updateCachingInterceptProvider)
                {
                    cachingInterceptorDescriptor.IgnoreWhenCacheKeyNull =
                        updateCachingInterceptProvider.IgnoreWhenCacheKeyNull;
                }

                foreach (var parameterDescriptor in Parameters)
                {
                    foreach (var cacheKey in parameterDescriptor.CacheKeys)
                    {

                        if (!cacheKey.Key.Equals(cachingInterceptorProvider.KeyTemplate))
                        {
                            continue;
                        }

                        foreach (var cacheKeyProvider in cacheKey.Value)
                        {
                            cachingInterceptorDescriptor
                            .CacheKeyProviderDescriptors
                            .Add(new CacheKeyProviderDescriptor()
                            {
                                PropName = cacheKeyProvider.PropName,
                                Index = cacheKeyProvider.Index,
                                CacheKeyType = cacheKeyProvider.CacheKeyType,
                                ParameterIndex = parameterDescriptor.Index,
                                From = parameterDescriptor.From,
                                IsSampleOrNullableType = parameterDescriptor.IsSingleType,
                            });
                        }
                    }
                }

                cachingInterceptorProvider.CachingInterceptorDescriptor = cachingInterceptorDescriptor;
                cachingInterceptorDescriptors.Add(cachingInterceptorDescriptor);
            }

            ServiceEntryDescriptor.CachingInterceptorDescriptors = cachingInterceptorDescriptors;
        }

        private FilterDescriptor[] CreateServerFilters()
        {
            var filterDescriptors = new List<FilterDescriptor>();
            var serviceEntryServerFilters = CustomAttributes.OfType<IServerFilterMetadata>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);

            foreach (var serviceEntryServerFilter in serviceEntryServerFilters)
            {
                filterDescriptors.Add(new FilterDescriptor(serviceEntryServerFilter, FilterScope.ServiceEntry));
            }

            var serviceServerFilters = ServiceType.GetCustomAttributes().OfType<IServerFilterMetadata>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);

            foreach (var serviceServerFilter in serviceServerFilters)
            {
                filterDescriptors.Add(new FilterDescriptor(serviceServerFilter, FilterScope.AppService));
            }

            var serverFilterFactories = EngineContext.Current.ResolveAll<IServerFilterFactory>();
            foreach (var serverFilterFactory in serverFilterFactories)
            {
                filterDescriptors.Add(new FilterDescriptor(serverFilterFactory, FilterScope.Global));
            }

            foreach (var serverFilter in EngineContext.Current.ApplicationOptions.Filter.Servers)
            {
                filterDescriptors.Add(new FilterDescriptor(serverFilter, FilterScope.Global));
            }

            return filterDescriptors.ToArray();
        }

        private FilterDescriptor[] CreateClientFilters()
        {
            var filterDescriptors = new List<FilterDescriptor>();
            var serviceEntryClientFilters = CustomAttributes.OfType<ClientFilterAttribute>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);
            foreach (var serviceEntryClientFilter in serviceEntryClientFilters)
            {
                filterDescriptors.Add(new FilterDescriptor(serviceEntryClientFilter, FilterScope.ServiceEntry));
            }

            var serviceClientFilters = ServiceType.GetCustomAttributes().OfType<ClientFilterAttribute>()
                .Where(p => p.GetType().IsClass && !p.GetType().IsAbstract);

            foreach (var serviceClientFilter in serviceClientFilters)
            {
                filterDescriptors.Add(new FilterDescriptor(serviceClientFilter, FilterScope.AppService));
            }

            var clientFilterFactories = EngineContext.Current.ResolveAll<IClientFilterFactory>();
            foreach (var clientFilterFactory in clientFilterFactories)
            {
                filterDescriptors.Add(new FilterDescriptor(clientFilterFactory, FilterScope.Global));
            }

            foreach (var clientFilter in EngineContext.Current.ApplicationOptions.Filter.Clients)
            {
                filterDescriptors.Add(new FilterDescriptor(clientFilter, FilterScope.Global));
            }

            filterDescriptors.Add(new FilterDescriptor(new RemoteInvokeBehavior(), FilterScope.ServiceEntry));

            return filterDescriptors.ToArray();
        }

        private IReadOnlyCollection<IAuthorizeData> CreateAuthorizeData()
        {
            var authorizeData = new List<IAuthorizeData>();
            var serviceEntryAuthorizeData = CustomAttributes.OfType<IAuthorizeData>();
            authorizeData.AddRange(serviceEntryAuthorizeData);
            var serviceAuthorizeData = ServiceType.GetCustomAttributes().OfType<IAuthorizeData>();
            authorizeData.AddRange(serviceAuthorizeData);

            if (!ServiceEntryDescriptor.IsAllowAnonymous
                && EngineContext.Current.ApplicationOptions.GlobalAuthorize &&
                !authorizeData.Any())
            {
                authorizeData.Add(new AuthorizeAttribute());
            }

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

            if (_serviceType.GetCustomAttributes().OfType<HashShuntStrategyAttribute>().Any() ||
                CustomAttributes.OfType<HashShuntStrategyAttribute>().Any())
            {
                GovernanceOptions.ShuntStrategy = ShuntStrategy.HashAlgorithm;
            }

            UpdateServiceEntryDescriptor(GovernanceOptions);
        }

        private void UpdateServiceEntryDescriptor(ServiceEntryGovernance serviceEntryGovernance)
        {
            ServiceEntryDescriptor.IsAllowAnonymous = serviceEntryGovernance.IsAllowAnonymous;
            ServiceEntryDescriptor.ProhibitExtranet = serviceEntryGovernance.ProhibitExtranet;
            ServiceEntryDescriptor.IsDistributeTransaction = this.IsTransactionServiceEntry();
            ServiceEntryDescriptor.WebApi = Router.RoutePath;
            ServiceEntryDescriptor.HttpMethod = Router.HttpMethod;
            ServiceEntryDescriptor.RouteOrder = Router.RouteTemplate.Order;
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
            if (Parameters.Any(p => p.From == ParameterFrom.Form) || this.NeedHttpProtocolSupport())
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

        public IReadOnlyList<RpcParameter> Parameters { get; }

        public IReadOnlyCollection<object> CustomAttributes { get; }

        public FilterDescriptor[] ClientFilters { get; private set; }

        public FilterDescriptor[] ServerFilters { get; private set; }

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