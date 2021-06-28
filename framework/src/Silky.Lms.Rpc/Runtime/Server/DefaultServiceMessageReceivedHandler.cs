using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Security;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.Rpc.Runtime.Server
{
    public class DefaultServiceMessageReceivedHandler : IServiceMessageReceivedHandler
    {
        private readonly IServiceEntryLocator _serviceEntryLocator;

        public DefaultServiceMessageReceivedHandler(IServiceEntryLocator serviceEntryLocator)
        {
            _serviceEntryLocator = serviceEntryLocator;
        }

        public async Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message)
        {
            RpcContext.GetContext()
                .SetAttachments(message.Attachments);
            
            RemoteResultMessage remoteResultMessage;
            try
            {
                var serviceEntry =
                    _serviceEntryLocator.GetLocalServiceEntryById(message.ServiceId);
                if (serviceEntry == null)
                {
                    throw new LmsException($"通过服务id{message.ServiceId}获取本地服务条目失败", StatusCode.NotFindLocalServiceEntry);
                }
                var tokenValidator = EngineContext.Current.Resolve<ITokenValidator>();
                if (!tokenValidator.Validate())
                {
                    throw new RpcAuthenticationException("rpc token不合法");
                }

                var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
                var result = await serviceEntry.Executor(currentServiceKey.ServiceKey,
                    message.Parameters);

                remoteResultMessage = new RemoteResultMessage()
                {
                    Result = result,
                    StatusCode = StatusCode.Success
                };
            }
            catch (Exception e)
            {
                remoteResultMessage = new RemoteResultMessage()
                {
                    ExceptionMessage = e.GetExceptionMessage(),
                    StatusCode = e.GetExceptionStatusCode(),
                    ValidateErrors = e.GetValidateErrors().ToArray()
                };
            }

            var resultTransportMessage = new TransportMessage(remoteResultMessage, messageId);
            await sender.SendAndFlushAsync(resultTransportMessage);
        }
    }
}