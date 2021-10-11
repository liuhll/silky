using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Rpc.Configuration;
using Silky.Rpc.Utils;
using Microsoft.Extensions.Options;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Silky.WebSocket
{
    public abstract class WsAppServiceBase : WebSocketBehavior
    {
        private WebSocketServer _wssr;
        protected static ConcurrentDictionary<string, IList<string>> BusinessSessionIds = new();
        protected static bool BusinessSessionIsSingleton { get; set; } = false;

        protected WsAppServiceBase()
        {
            _wssr = EngineContext.Current.Resolve<WebSocketServer>();
        }

        protected override void OnOpen()
        {
            ValidateToken();
            var businessId = Context.Headers["businessId"];
            if (!businessId.IsNullOrEmpty())
            {
                var businessSessionId = BusinessSessionIds.GetValueOrDefault(businessId, new List<string>());
                Debug.Assert(businessSessionId != null);
                if (BusinessSessionIsSingleton && businessSessionId.Count >= 1)
                {
                    Context.WebSocket.Send($"系统中已经存在{businessId}的会话");
                    Context.WebSocket.Close();
                }

                businessSessionId.Add(ID);
                BusinessSessionIds.AddOrUpdate(businessId, businessSessionId, (key, val) => businessSessionId);
            }
        }

        protected void ValidateToken()
        {
            var wsToken = Context.Headers["wsToken"];
            if (wsToken == null)
            {
                Context.WebSocket.Close();
                throw new SilkyException("You did not specify wsToken, please link with ws service through gateway",
                    StatusCode.RpcUnAuthentication);
            }

            var webSocketOptions = EngineContext.Current.Resolve<IOptions<WebSocketOptions>>().Value;
            if (webSocketOptions.Token != wsToken)
            {
                Context.WebSocket.Close();
                throw new SilkyException("The rpc token you specified is incorrect", StatusCode.RpcUnAuthentication);
            }
        }

        /// <summary>
        /// During RPC communication, there is no session with WS client.
        /// You can get all existing sessions of WS service through <code>SessionManager</code>
        /// You can associate sessionId with businessId to the when <code>onOpen</code>, cache it through static dictionary.
        /// and then get sessionId through businessId during RPC communication
        /// </summary>
        public WebSocketSessionManager SessionManager
        {
            get
            {
                WebSocketSessionManager result = null;
                var wsPath = WebSocketResolverHelper.ParseWsPath(GetType().GetTypeInfo());
                if (_wssr.WebSocketServices.TryGetServiceHost(wsPath, out var webSocketServiceHost))
                    result = webSocketServiceHost.Sessions;
                return result;
            }
        }
    }
}