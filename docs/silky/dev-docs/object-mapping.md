---
title: 对象到对象的映射
lang: zh-cn
---

## 对象映射的概念

将一个对象的数据根据特定规则批量映射到另一个对象中，减少手工操作和降低人为出错率。如将 DTO 对象映射到 Entity 实体中，反之亦然。

silky框架使用[AutoMapper](https://github.com/AutoMapper/AutoMapper)包作为对象映射工具。

后期,silky框架也准备扩展使用[Mapster](https://github.com/MapsterMapper/Mapster)包作为对象映射工具。

## 用法

### 使用AutoMapper作为映射工具

1. 在在启动模块(StartUpModule)中，显式的依赖`AutoMapperModule`模块。
   
   如果启动模块指定的是`NormHostModule`,那么,该模块已经指定依赖`AutoMapperModule`模块。

2. 通过继承`Profile`基类,在其构造器中指定`源`与`目的`的类型映射关系。
  
  例如:

```csharp
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<CreateAccountInput, Domain.Accounts.Account>();
            CreateMap<Domain.Accounts.Account, GetAccountOutput>();
            CreateMap<UpdateAccountInput, Domain.Accounts.Account>().AfterMap((src, dest) =>
            {
                dest.UpdateTime = DateTime.Now;
                dest.UpdateBy = NullSession.Instance.UserId;
            });
        }
    }
```

3. 通过`MapTo`方法实现对象属性映射

```csharp
public async Task<GetAccountOutput> Create(CreateAccountInput input)
{
   // 输入类型对象映射为实体
   var account = input.MapTo<Domain.Accounts.Account>();
   account = await _accountDomainService.Create(account);
   // 实体对象映射为输出对象   
   return account.MapTo<GetAccountOutput>();
}
```

```csharp
public async void Update(UpdateAccountInput input)
{
   var account = await GetAccountById(input.Id);
   account = input.MapTo(account); //通过输入对象更新实体属性
}
```
