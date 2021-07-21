---
title: silky框架分布式事务使用简介
lang: zh-cn
---

silky框架的分布式事务解决方案采用的TCC事务模型。在开发过程中参考和借鉴了[hmily](https://github.com/dromara/hmily)。使用AOP的编程思想,在rpc通信过程中通过拦截器的方式对全局事务或是分支事务进行管理和协调。

本文通过silky.samples 订单接口给大家介绍silky框架分布式事务的基本使用。


## silky分布式事务的使用

在silky框架中,在应用服务接口通过`[Transaction]`特性标识该接口是一个分布式事务接口(应用接口层需要安装包`Silky.Transaction`)。应用服务接口的实现必须需要通过`   [TccTransaction(ConfirmMethod = "ConfirmMethod", CancelMethod = "CancelMethod")]`特性指定Confirm阶段和Cancel阶段的方法(需要再应用层安装包`Silky.Transaction.Tcc`)。

::: warning 注意

一个应用接口被分布式事务`[Transaction]`特性标识,那么这个应用接口的实现也必须要使用`TccTransaction`特性来标识。否则,应用在启动时会抛出异常。

:::


在一个分布式事务处理过程中,会存在如下两种角色的事务。

### 事务角色

1. 全局事务

在Silky框架中,第一个执行的事务被认为是全局事务(事务角色为`TransactionRole.Start`)。换句话说,在一个业务处理过程中,执行的第一个被标识为`TccTransaction`(应用接口需要被标识为`Transaction`)的方法为全局事务。

当然,全局事务也作为事务的一特殊的事务参与者,在全局事务开始后,作为事务参与者注册到事务上下文中。

2. 分支事务

在开始的一个分布式事务中,参与rpc通信,且被特性`[Transaction]`标识的应用服务,被认为是分支事务(事务角色为:`TransactionRole.Participant`)。

### 事务的执行

1. 在开启一个全局事务之后,在全局事务的`try`过程中，首先将全局事务作为一个事务参与者添加到事务上下文中。如果遇到一个分支事务,那么首先会调用分支事务的`try`方法。如果`try`方法执行成功，那么分支事务作为一个事务参与者被注册到事务上下文中,并且分支的事务状态为变更为`trying`。
 
2. 如果在全局事务的try方法执行过程中发生异常,那么全局事务的`Cancel`方法和被加入事务上下文且状态为`trying`的分支事务参与者的`Cancel`方法将会被调用,在`Cancel`方法中实现数据回滚。也就是说,全局事务的`Cancel`不管`try`方法是否执行成功,全局事务的`Cancel`方法都会被执行。分支事务只有被加入到事务上下文,且状态为`trying`(分支事务已经执行过`try`方法),那么分支事务的`Cancel`方法才会被执行。
 
3. 全局事务的try方法执行成功,那么全局事务的`Confirm`和各个分支事务的`Confirm`方法将会得到执行。

4. 换句话说,所有全局事务(事务主分支)以及分支事务的try方法都执行成功,才会依次执行所有事务参与者的`Confirm`方法,如果分布式事务的`try`阶段执行失败,那么主分支事务的`Cancel`方法一定会被调用;而分支事务看是否有被添加到事务上下文中且已经执行成功`try`阶段的方法,只有这样的分支事务才会调用`Cancel`方法。

5. 如果分支事务存在分支事务的情况下,这种业务场景会相对特殊,这个时候的分支事务相对于它的分支事务就是一个特殊的全局事务。它会在特殊的`try`阶段执行孙子辈的分支事务的`try`和`confirm`(成功)或是`try`和`cancel`(失败)。并且会将执行成功与否返回给父分支事务(全局事务)。

::: warning 注意

无论是全局事务还是分支事务的各个阶段,如果涉及到多个表的操作,那么,对应的数据库操作的都需要放到本地事务进行操作。

:::

## 分布式事务案例-- silky.samples订单接口

下面,我们通过silky.samples的订单接口来熟悉通过silky框架如何实现分布式事务。

### silky.samples 订单接口的业务流程介绍

在上一篇博文[通过silky.samples熟悉silky微服务框架的使用](https://www.cnblogs.com/bea084100123/p/14631609.html)，给大家介绍了silky.samples样例项目的基本情况。本文通过大家熟悉的一个订单接口,熟悉silky的分布式事务是如何使用。

下面，给大家梳理一下订单接口的业务流程。

1. 判断和锁定订单产品库存: 在下订单之前需要判断是否存在相应的产品,产品的剩余数量是否足够，如果产品数量足够的话,扣减产品库存,锁定订单的库存数量(分支事务)

2. 创建一个订单记录,订单状态为NoPay(全局事务)

3. 判断用户的账号是否存在,账户余额是否充足,如果账户余额充足的话,则需要锁定订单金额，创建一个账户流水记录。

4. 如果1,2,3都成功,释放产品锁定的订单库存

5. 如果1,2,3都成功,释放账号锁定的金额,修改账号流水记录相关状态

6. 如果1,2,3都成功,修改订单状态为Payed

7. 如果在**步骤1**就出现异常(例如:产品的库存不足或是rpc通信失败,或是访问数据库出现异常等),库存分支事务(`DeductStockCancel`)和账号分支事务(`DeductBalanceCancel`)指定的`Cancel`方法都不会被执行。但是全局事务指定的`Cancel`方法(`OrderCreateCancel`)会被调用

8. 如果在**步骤2**就出现异常(下订单访问数据库出现异常),库存分支事务指定的`Cancel`方法(`DeductStockCancel`)以及全局事务指定的`Cancel`方法(`OrderCreateCancel`)会被调用，账号分支事务指定(`DeductBalanceCancel`)的`Cancel`方法都不会被执行。

9. 如果在**步骤3**就出现异常(用户的账号余额不足,访问数据库出现异常等),那么库存分支事务(`DeductStockCancel`)和账号分支事务指定(`DeductBalanceCancel`)全局事务指定的`Cancel`方法(`OrderCreateCancel`)都会被调用。

::: tip 提示

1. 如果在一个分布式事务处理失败,全局事务的`Cancel`方法一定会被调用。分支事务的`Try`方法得到执行(分支事务的状态为`trying`),那么将会执行分支事务指定的`Cancel`方法。如果分支事务的分支事务的`Try`方法没有得到执行(分支事务的状态为`pretry`),那么不会执行分支事务指定的`Cancel`方法。

2. 上述的业务流程过程中,步骤1,2,3为`try`阶段,步骤4,5,6为`confirm`阶段,步骤7,8,9为`concel`阶段。

:::

### 全局事务--订单接口

通过[silky分布式事务的使用](#silky分布式事务的使用)节点的介绍,我们知道在服务之间的rpc通信调用中,执行的第一个被标识为`Transaction`的应用方法即为全局事务(即:事务的开始)。

首先， 我们需要在订单应用接口中通过`[Transaction]`来标识这是一个分布式事务的应用接口。

```csharp
  [Transaction]
  Task<GetOrderOutput> Create(CreateOrderInput input);
```

其次,在应用接口的实现通过`[TccTransaction]`特性指定`ConfirmMethod`方法和`CancelMethod`。
- 指定的`ConfirmMethod`和`CancelMethod`必须为`public`类型,但是不需要在应用接口中声明。
- 全局事务的`ConfirmMethod`和`CancelMethod`必定有一个会被执行,如果try方法(`Create`)执行成功,那么执行`ConfirmMethod`方法,执行失败,那么则会执行`CancelMethod`。
- 可以将`try`、`confirm`、`cancel`阶段的方法放到领域服务中实现。
- 全局事务可以通过`RpcContext`的`Attachments`向分支事务或是`confirm`、`cancel`阶段的方法传递Attachment参数。但是分支事务不能够通过`RpcContext`的`Attachments`向全局事务传递Attachment参数。

```csharp
/// <summary>
/// try阶段的方法
/// </summary>
/// <param name="input"></param>
/// <returns></returns>
[TccTransaction(ConfirmMethod = "OrderCreateConfirm", CancelMethod = "OrderCreateCancel")]
public async Task<GetOrderOutput> Create(CreateOrderInput input)
{
    return await _orderDomainService.Create(input); //具体的业务放到领域层实现
}

// confirm阶段的方法
public async Task<GetOrderOutput> OrderCreateConfirm(CreateOrderInput input)
{
    var orderId = RpcContext.GetContext().GetAttachment("orderId");
    var order = await _orderDomainService.GetById(orderId.To<long>());
    order.Status = OrderStatus.Payed;
    order = await _orderDomainService.Update(order);
    return order.MapTo<GetOrderOutput>();
}

// cancel阶段的方法
public async Task OrderCreateCancel(CreateOrderInput input)
{
    var orderId = RpcContext.GetContext().GetAttachment("orderId");
    // 如果不为空证明已经创建了订单
    if (orderId != null)
    {
        // 是否保留订单可以根据具体的业务来确定。
        // await _orderDomainService.Delete(orderId.To<long>());
        var order = await _orderDomainService.GetById(orderId.To<long>());
        order.Status = OrderStatus.UnPay;
        await _orderDomainService.Update(order);
    }
}

```

下订单的具体业务(订单try阶段的实现)

```csharp
public async Task<GetOrderOutput> Create(CreateOrderInput input)
{
    // 扣减库存
    var product = await _productAppService.DeductStock(new DeductStockInput()
    {
        Quantity = input.Quantity,
        ProductId = input.ProductId
    }); // rpc调用,DeductStock被特性[Transaction]标记,是一个分支事务

    // 创建订单
    var order = input.MapTo<Domain.Orders.Order>();
    order.Amount = product.UnitPrice * input.Quantity;
    order = await Create(order);
    RpcContext.GetContext().SetAttachment("orderId", order.Id); //分支事务或是主分支事务的confirm或是cancel阶段可以从RpcContext获取到Attachment参数。

    //扣减账户余额
    var deductBalanceInput = new DeductBalanceInput()
        {OrderId = order.Id, AccountId = input.AccountId, OrderBalance = order.Amount};
    var orderBalanceId = await _accountAppService.DeductBalance(deductBalanceInput); // rpc调用,DeductStock被特性[Transaction]标记,是一个分支事务
    if (orderBalanceId.HasValue)
    {
        RpcContext.GetContext().SetAttachment("orderBalanceId", orderBalanceId.Value);//分支事务或是主分支事务的confirm或是cancel阶段可以从RpcContext获取到Attachment参数。
    }

    return order.MapTo<GetOrderOutput>();
}
```


### 分支事务--扣减库存

首先,需要在应用接口层标识这个是一个分布式事务接口。

```csharp
// 标识这个是一个分布式事务接口
[Transaction]
// 执行成功,清除缓存数据
[RemoveCachingIntercept("GetProductOutput","Product:Id:{0}")]
// 该接口不对集群外部发布
[Governance(ProhibitExtranet = true)]
Task<GetProductOutput> DeductStock(DeductStockInput input);
```

其次,应用接口的实现指定`Confirm`阶段和`Cancel`阶段的方法。

```csharp
[TccTransaction(ConfirmMethod = "DeductStockConfirm", CancelMethod = "DeductStockCancel")]
public async Task<GetProductOutput> DeductStock(DeductStockInput input)
{
    var product = await _productDomainService.GetById(input.ProductId);
    if (input.Quantity > product.Stock)
    {
        throw new BusinessException("订单数量超过库存数量,无法完成订单");
    }

    product.LockStock += input.Quantity;
    product.Stock -= input.Quantity;
    product = await _productDomainService.Update(product);
    return product.MapTo<GetProductOutput>();
  
}

public async Task<GetProductOutput> DeductStockConfirm(DeductStockInput input)
{
    //Confirm阶段的具体业务放在领域层实现
    var product = await _productDomainService.DeductStockConfirm(input);
    return product.MapTo<GetProductOutput>();
}

public Task DeductStockCancel(DeductStockInput input)
{
    //Cancel阶段的具体业务放在领域层实现
    return _productDomainService.DeductStockCancel(input);
   
}
```

### 分支事务--扣减账户余额

首先,需要在应用接口层标识这个是一个分布式事务接口。

```csharp
 [Governance(ProhibitExtranet = true)]
[RemoveCachingIntercept("GetAccountOutput","Account:Id:{0}")]
[Transaction]
Task<long?> DeductBalance(DeductBalanceInput input);
```


其次,应用接口的实现指定`Confirm`阶段和`Cancel`阶段的方法。

```csharp
[TccTransaction(ConfirmMethod = "DeductBalanceConfirm", CancelMethod = "DeductBalanceCancel")]
public async Task<long?> DeductBalance(DeductBalanceInput input)
{
    var account = await _accountDomainService.GetAccountById(input.AccountId);
    if (input.OrderBalance > account.Balance)
    {
        throw new BusinessException("账号余额不足");
    }
    return await _accountDomainService.DeductBalance(input, TccMethodType.Try);
}

public Task DeductBalanceConfirm(DeductBalanceInput input)
{
    return _accountDomainService.DeductBalance(input, TccMethodType.Confirm);
}

public Task DeductBalanceCancel(DeductBalanceInput input)
{
    return _accountDomainService.DeductBalance(input, TccMethodType.Cancel);
}
  
```

第三, 领域层的业务实现

```csharp
 public async Task<long?> DeductBalance(DeductBalanceInput input, TccMethodType tccMethodType)
 {
    var account = await GetAccountById(input.AccountId);
    //涉及多张表,所有每一个阶段的都放到一个本地事务中执行  
    var trans = await _repository.BeginTransactionAsync();
     BalanceRecord balanceRecord = null;
     switch (tccMethodType)
     {
         case TccMethodType.Try:
             account.Balance -= input.OrderBalance;
             account.LockBalance += input.OrderBalance;
             balanceRecord = new BalanceRecord()
             {
                 OrderBalance = input.OrderBalance,
                 OrderId = input.OrderId,
                 PayStatus = PayStatus.NoPay
             };
             await _repository.InsertAsync(balanceRecord);
             RpcContext.GetContext().SetAttachment("balanceRecordId",balanceRecord.Id);
             break;
         case TccMethodType.Confirm:
             account.LockBalance -= input.OrderBalance;
             var balanceRecordId1 = RpcContext.GetContext().GetAttachment("orderBalanceId")?.To<long>();
             if (balanceRecordId1.HasValue)
             {
                 balanceRecord = await _repository.GetByIdAsync<BalanceRecord>(balanceRecordId1.Value);
                 balanceRecord.PayStatus = PayStatus.Payed;
                 await _repository.UpdateAsync(balanceRecord);
             }
             break;
         case TccMethodType.Cancel:
             account.Balance += input.OrderBalance;
             account.LockBalance -= input.OrderBalance;
             var balanceRecordId2 = RpcContext.GetContext().GetAttachment("orderBalanceId")?.To<long>();
             if (balanceRecordId2.HasValue)
             {
                 balanceRecord = await _repository.GetByIdAsync<BalanceRecord>(balanceRecordId2.Value);
                 balanceRecord.PayStatus = PayStatus.Cancel;
                 await _repository.UpdateAsync(balanceRecord);
             }
             break;
     }

    
     await _repository.UpdateAsync(account);
     await trans.CommitAsync();
     // 将受影响的缓存数据移除。
     await _accountCache.RemoveAsync($"Account:Name:{account.Name}");
     return balanceRecord?.Id;
 }
```

## 订单接口测试

**前提**

存在如下账号和产品:

![tcc-account.png](/assets/imgs/tcc-account.png)

![tcc-product.png](/assets/imgs/tcc-product.png)

### 模拟库存不足

**请求参数:**

```json
{
  "accountId": 1,
  "productId": 1,
  "quantity": 11
}
```

**响应:**

```json
{
  "data": null,
  "status": 1000,
  "statusCode": "BusinessError",
  "errorMessage": "订单数量超过库存数量,无法完成订单",
  "validErrors": null
}
```

**数据库变化**

查看数据库,并没有生成订单信息,账户余额和产品库存也没有修改：

![tcc-account1.png](/assets/imgs/tcc-account1.png)

![tcc-product1.png](/assets/imgs/tcc-product1.png)

**测试结果:**

库存和账户余额均为变化,也未创建订单信息

达到期望

### 模拟账号余额不足

**请求参数:**

```json
{
  "accountId": 1,
  "productId": 1,
  "quantity": 9
}
```

**响应:**

```json
{
  "data": null,
  "status": 1000,
  "statusCode": "BusinessError",
  "errorMessage": "账号余额不足",
  "validErrors": null
}
```

**数据库变化**

1. 新增了一个产品订单,订单状态为未支付状态
![tcc-order2.png](/assets/imgs/tcc-order2.png)

2. 产品库存和账户余额并未变更
![tcc-account2.png](/assets/imgs/tcc-account2.png)

![tcc-product2.png](/assets/imgs/tcc-product2.png)

**测试结果:**

创建了一个新的订单,状态为未支付,用户账号余额,产品订单均未变化。

达到测试期望

### 正常下订单

```json
{
  "accountId": 1,
  "productId": 1,
  "quantity": 2
}
```

**响应:**

```json
{
  "data": {
    "id": 2,
    "accountId": 1,
    "productId": 1,
    "quantity": 2,
    "amount": 20,
    "status": 1
  },
  "status": 200,
  "statusCode": "Success",
  "errorMessage": null,
  "validErrors": null
}
```

**数据库变化**

1. 创建了一个订单,该订单状态为已支付

![tcc-product3.png](/assets/imgs/tcc-order3.png)

2. 库存扣减成功

![tcc-product3.png](/assets/imgs/tcc-product3.png)

3. 账户金额扣减成功,并且创建了一个流水记录

![tcc-account3.png](/assets/imgs/tcc-account3.png)

![tcc-balance-record3.png](/assets/imgs/tcc-balance-record3.png)

**测试结果:**

创建了一个新的订单,状态为支付,用户账号余额,产品订单均被扣减,且也创建了交易流水记录。

达到期望结果。