---
title: websocket通信
lang: zh-cn
---

## 简介

silky框架通过[WebSocketSharp](https://github.com/sta/websocket-sharp)支持与微服务应用建立websocket会话。silky框架网关实现了支持websocket通信的代理中间件，可以通过网关地址与websocket服务建立会话。在与websocket服务建立会话时,需要指定通过请求头或是`qString`设置一个`bussinessId`参数,通过该参数与建立会话的`SessionId`进行关联,这样,在与其他服务进行rpc通信过程中,就可以通过`bussinessId`找到与前端建里会话的`sessionId`。如果找到`sessionId`说明与前端建立了websocket会话,服务端就可以将消息推送到客户端。

## 建立支持websocket通信的主机

微服务应用想要支持websocket会话,需要构建主机时，启动模块依赖`WebSocketModule`模块。开发者可以在构造寄宿主机时将启动模块直接指定为`WsHostModule`,或是在自定义的启动模块中指定依赖`WebSocketModule`。

```csharp
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .RegisterSilkyServices<WsHostModule>()
                ;
        }
    }
```

## 建立websocket会话

1. 微服务应用服务实现除了需要继承应用服务接口之外,还需要继承`WsAppServiceBase`基类

  应用服务接口的定义与其他类型的应用服务接口定义相同,应用服务接口方法可以与其他微服务应用进行rpc通信。

  应用服务接口的定义：
  
  ```csharp
    [ServiceRoute]
    public interface IWsTestAppService
    {
        Task Echo(string businessId,string msg);
    }
  ```

  应用服务的实现类:
  
  ```csharp
    public class WsTestAppService : WsAppServiceBase, IWsTestAppService
    {
        public async Task Echo(string businessId, string msg)
        {
            if (BusinessSessionIds.TryGetValue(businessId,out var sessionIds))
            {
                foreach (var sessionId in sessionIds)
                {
                    SessionManager.SendTo($"message:{msg},sessionId:{sessionId}", sessionId);
                }
            }
            else
            {
                throw new BusinessException($"不存在businessId为{businessId}的会话");
            }
        }
    }
  ```

2. 网关应用引用应用服务接口,通过网关地址和websocket服务api与客户端建立会话
   
   需要特别强调的是在与服务端建立websocket会话时,必须要通过请求头或是`qString`设置一个`bussinessId`参数,否则无法与服务端建立会话。建立会话的格式如下所示:
   
   在建立会话后,`sessionID`与`businessId`的关系会缓存在字典`BusinessSessionIds`中(`key`为`bussinessId`,`value`为建立的`sessionId`)。在建立会后,定义的应用服务接口可以将`bussinessId`作为方法的参数,这样,就可以通过`BusinessSessionIds`字典中的对应关系找到session会话,通过会话的session推送给客户端消息。

   

   ```
   ws://gatewayip:port/[api]/websocketName?bussinessId=bussinessId
   ```

   例如,上述实例中,与websocket服务建立会话的api为:
   
   ```
   ws://127.0.0.1:5000/api/wstest?businessid=100
   ```

   ![ws1.png](/assets/imgs/ws1.png)

   通过webapi模拟服务获取消息后推送给websocket客户端消息：

   ![ws2.png](/assets/imgs/ws2.png)

   websocket接收到服务端推送的消息：

   ![ws3.png](/assets/imgs/ws3.png)

  如果未建立会话,则发生异常：
  
   ![ws4.png](/assets/imgs/ws4.png)