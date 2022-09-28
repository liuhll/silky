using Silky.Core;
using Silky.Core.Convertible;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Transport.Messages
{
    public static class RemoteResultMessageExtensions
    {
        private static IServiceEntryLocator _serviceEntryLocator;
        private static ITypeConvertibleService _typeConvertibleService;

        static RemoteResultMessageExtensions()
        {
            _serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
            _typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        }

        public static object GetResult(this RemoteResultMessage remoteResultMessage)
        {
            var result = remoteResultMessage.Result;
            if (result == null)
            {
                return result;
            }

            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteResultMessage.ServiceEntryId);
            if (serviceEntry == null)
            {
                return result;
            }
            result = remoteResultMessage.Result.GetType() == serviceEntry.ReturnType
                ? remoteResultMessage.Result
                : _typeConvertibleService.Convert(remoteResultMessage.Result, serviceEntry.ReturnType);

            return result;
        }
    }
}