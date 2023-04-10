---
title: 身份认证与授权
lang: zh-cn
---

## 生成JWT token

silky提供了`IJwtTokenGenerator`接口用于生成Jwt Token,您可以通过如下步骤实现登录接口:

开发者可以在**账号管理微服务**中,来定义和生成token服务和方法;

1. 通过`IServiceCollection`注入JWT相关服务;

```csharp
    public class ConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddJwt();
            // 注入其他服务
        }
    }
```

2. 指定生成Jwt Token的配置,打开`appsettings.yaml`文件,添加如下配置:

```yaml
jwtSettings:
  secret: jaoaNPA1fo1rcPfK23iNufsQKkhTy8eh # token 密钥
  issuer: http://silky.com/issuer # 缺省值为http://silky.com/issuer
  audience: http://silky.com/audience # 缺省值为http://silky.com/audience
  expiredTime: 24 # token过期时间, 单位:小时,缺省值为24
  algorithm: HS256 # token 加密算法
```

3. 开发者可以定义账户服务和登录方法,用于生成token

3.1 定义账号服务接口和登录接口

```csharp

   // 定义账号服务接口和登录方法
    [ServiceRoute]
    public interface IAccountAppService
    {

        // 登录方法
        // 登录方法要求所有用户均可以访问,所以需要特性[AllowAnonymous] 标识
        [AllowAnonymous]
        Task<string> Login(LoginInput input);
    }
```

3.2 实现账号服务接口和登录接口
```csharp
   // 账号接口的实现
   public class AccountAppService : IAccountAppService
   {

        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AccountAppService(IJwtTokenGenerator jwtTokenGenerator)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> Login(LoginInput input)
        {

            // todo 根据实际业务情况实现密码校验
            if (!input.Password.Equals("123qwe"))
            {
                throw new AuthenticationException("密码不正确");
            }
            var userId = 1; //  todo 从数据中查询到登录用户和用户Id

            // 指定token payload字典
            var payload = new Dictionary<string, object>()
            {
                { ClaimTypes.NameIdentifier, userId },
                { ClaimTypes.Name, input.UserName },
                { ClaimTypes.Role, "PowerUser,Dashboard" }
            };

            var token = _jwtTokenGenerator.Generate(payload); // 生成token
            RpcContext.Context.SigninToSwagger(token); // 通过响应头 access-token 指定token,swagger可以自动登录
            return token; // 返回jwt token
        }

   }

```

上述代码中,开发者可以根据实际情况对`Login()`方法的业务逻辑进行调整,代码编写完成后,可以通过网关访问WebAPI`/account/login-POST`实现登录,登录成功后,即可获取JWT token

![identity1.png](/assets/imgs/identity1.png)

![identity2.png](/assets/imgs/identity2.png)



##  通过网关实现统一的身份认证

Silky对于WebAPI请求的身份认证是通过网关统一进行的,网关是http请求的统一入口,当http请求到达网关后,网关的身份认证中间件对请求头携带的Token进行认证，如果token合法才被放行;


### AuthorizeAttribute 特性

### AllowAnonymous 特性

## 获取当前登陆用户信息

## 如何实现接口授权

## RPC token 校验