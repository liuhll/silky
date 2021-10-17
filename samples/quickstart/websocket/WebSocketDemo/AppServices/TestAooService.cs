using Application.Contracts.AppServices;
using Microsoft.Extensions.Logging;
using Silky.WebSocket;
using WebSocketSharp;

namespace WebSocketDemo.AppServices
{
    public class TestAooService : WsAppServiceBase, ITestAppService
    {
        private readonly ILogger<TestAooService> _logger;

        public TestAooService(ILogger<TestAooService> logger)
        {
            _logger = logger;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            _logger.LogInformation("websocket established a session");
            
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            _logger.LogInformation("websocket disconnected");
        }
    }
}