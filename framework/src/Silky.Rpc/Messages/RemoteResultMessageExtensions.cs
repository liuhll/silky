using System;
using Newtonsoft.Json.Linq;
using Silky.Core;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Messages
{
    public static class RemoteResultMessageExtensions
    {
        private static ISerializer _serializer;
        private static IServiceEntryLocator _serviceEntryLocator;

        static RemoteResultMessageExtensions()
        {
            _serializer = EngineContext.Current.Resolve<ISerializer>();
            _serviceEntryLocator = EngineContext.Current.Resolve<IServiceEntryLocator>();
        }

        public static object GetResult(this RemoteResultMessage remoteResultMessage)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(remoteResultMessage.ServiceId);

            if (remoteResultMessage.Result.GetType() == serviceEntry.ReturnType)
            {
                return remoteResultMessage.Result;
            }

            return _serializer.Deserialize(serviceEntry.ReturnType, remoteResultMessage.Result.ToString());
        }
    }
}