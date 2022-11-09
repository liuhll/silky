using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Reflection;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Extensions;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceHelper
    {
        private static readonly string RpcAppService = "Silky.Rpc";
        
        internal static IEnumerable<Type> FindLocalServiceTypes(ITypeFinder typeFinder)
        {
            var types = typeFinder.GetaAllExportedTypes()
                    .Where(p => p.IsClass
                                && !p.IsAbstract
                                && !p.IsGenericType
                                && p.GetInterfaces().Any(i =>
                                    i.GetCustomAttributes().Any(a => a is ServiceRouteAttribute))
                    )
                    .OrderBy(p =>
                        p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Select(q => q.Weight).FirstOrDefault()
                    )
                ;
            return types;
        }

        internal static IEnumerable<Type> FindLocalServiceImplementTypes(ITypeFinder typeFinder, Type type)
        {
            var types = typeFinder.GetaAllExportedTypes()
                    .Where(p => p.IsClass
                                && !p.IsAbstract
                                && !p.IsGenericType
                                && p.GetInterfaces().Any(i =>
                                    i.GetCustomAttributes().Any(a => a is ServiceRouteAttribute)
                                    && i == type
                                )
                    )
                    .OrderBy(p =>
                        p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Select(q => q.Weight).FirstOrDefault()
                    )
                ;
            return types;
        }

        public static IEnumerable<(Type, bool)> FindAllServiceTypes(ITypeFinder typeFinder)
        {
            var serviceTypes = new List<(Type, bool)>();
            var exportedTypes = typeFinder.GetaAllExportedTypes();
            var serviceInterfaces = exportedTypes
                    .Where(p => p.IsInterface
                                && p.GetCustomAttributes().Any(a => a is ServiceRouteAttribute)
                                && !p.IsGenericType
                    )
                ;
            foreach (var entryInterface in serviceInterfaces)
            {
                serviceTypes.Add(
                    exportedTypes.Any(t => entryInterface.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        ? (entryInterface, true)
                        : (entryInterface, false));
            }

            return serviceTypes;
        }

        
        public static IEnumerable<Assembly> ReadInterfaceAssemblies()
        {
            return FindAllServiceTypes(EngineContext.Current.TypeFinder).Select(p => p.Item1)
                .Where(p => !p.Assembly.FullName.Contains(RpcAppService))
                .GroupBy(p => p.Assembly)
                .Select(p => p.Key);
        }

        public static IEnumerable<Type> FindServiceProxyTypes(ITypeFinder typeFinder)
        {
            var proxyTypes = FindAllServiceTypes(typeFinder).Where(p => !p.Item2)
                .Select(p => p.Item1);
            return proxyTypes;
        }

        public static IEnumerable<(Type, bool)> FindWsServiceTypeInfos(ITypeFinder typeFinder)
        {
            var entryTypes = new List<(Type, bool)>();
            var exportedTypes = typeFinder.GetaAllExportedTypes();

            var entryInterfaces = exportedTypes
                    .Where(p => p.IsInterface
                                && p.GetCustomAttributes().Any(a =>
                                    (a is IRouteTemplateProvider))
                                && !p.IsGenericType
                    )
                ;
            foreach (var entryInterface in entryInterfaces)
            {
                entryTypes.Add(exportedTypes.Any(t => entryInterface.IsAssignableFrom(t)
                                                      && t.IsClass
                                                      && t.BaseType?.FullName ==
                                                      "Silky.WebSocket.WsAppServiceBase"
                                                      && !t.IsAbstract)
                    ? (entryInterface, true)
                    : (entryInterface, false));
            }

            return entryTypes;
        }

        public static IEnumerable<Type> FindServiceLocalWsTypes(ITypeFinder typeFinder)
        {
            var types = typeFinder.GetaAllExportedTypes()
                    .Where(p => p.IsClass
                                && !p.IsAbstract
                                && !p.IsGenericType
                                && p.GetInterfaces().Any(i =>
                                    i.GetCustomAttributes().Any(a => a is ServiceRouteAttribute))
                                && p.BaseType?.FullName == ServiceConstant.WebSocketBaseTypeName
                    )
                    .OrderBy(p =>
                        p.GetCustomAttributes().OfType<ServiceKeyAttribute>().Select(q => q.Weight).FirstOrDefault()
                    )
                ;
            return types;
        }

        public static bool IsWsType(Type type)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                return type.GetInterfaces().Any(t => t.GetCustomAttributes().Any(a => a is ServiceRouteAttribute)) &&
                       type.BaseType?.FullName == ServiceConstant.WebSocketBaseTypeName;
            }

            // if (type.IsInterface)
            // {
            //     var localServiceImplementTypes = FindLocalServiceImplementTypes(EngineContext.Current.TypeFinder, type);
            //     return localServiceImplementTypes.Any(implementType =>
            //         implementType.GetInterfaces()
            //             .Any(t => t.GetCustomAttributes().Any(a => a is ServiceRouteAttribute)) &&
            //         implementType.BaseType?.FullName == ServiceConstant.WebSocketBaseTypeName);
            // }

            return false;
        }

        public static ServiceProtocol GetServiceProtocol(Type type, bool isLocal, bool isService)
        {
            if (isLocal && isService && IsWsType(type))
            {
                return ServiceProtocol.Ws;
            }

            return ServiceProtocol.Rpc;
        }
    }
}