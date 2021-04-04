using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.AutoMapper;
using Lms.Core.Extensions;
using Lms.Order.Application.Contracts.Orders;
using Lms.Order.Application.Contracts.Orders.Dtos;
using Lms.Order.Domain.Orders;
using Lms.Order.Domain.Shared.Orders;
using Lms.Rpc.Transport;
using Lms.Stock.Application.Contracts.Products;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Transaction.Tcc;

namespace Lms.Order.Application.Orders
{
    public class OrderAppService : IOrderAppService
    {
        private readonly IOrderDomainService _orderDomainService;
        private readonly IAccountAppService _accountAppService;
        private readonly IProductAppService _productAppService;

        public OrderAppService(IOrderDomainService orderDomainService,
            IAccountAppService accountAppService,
            IProductAppService productAppService)
        {
            _orderDomainService = orderDomainService;
            _accountAppService = accountAppService;
            _productAppService = productAppService;
        }

        [TccTransaction(ConfirmMethod = "OrderCreateConfirm", CancelMethod = "OrderCreateCancel")]
        public async Task<GetOrderOutput> Create(CreateOrderInput input)
        {
            // 扣减库存
            var product = await _productAppService.DeductStock(new DeductStockInput()
            {
                Quantity = input.Quantity,
                ProductId = input.ProductId
            });
            
            // 创建订单
            var order = input.MapTo<Domain.Orders.Order>();
            order.Amount = product.UnitPrice * input.Quantity;
            order = await _orderDomainService.Create(order);
            RpcContext.GetContext().SetAttachment("orderId", order.Id);

            //扣减账户余额
            var deductBalanceInput = new DeductBalanceInput()
                {OrderId = order.Id, AccountId = input.AccountId, OrderBalance = order.Amount};
            await _accountAppService.DeductBalance(deductBalanceInput);
            return order.MapTo<GetOrderOutput>();
        }

        public async Task<GetOrderOutput> OrderCreateConfirm(CreateOrderInput input)
        {
            var orderId = RpcContext.GetContext().GetAttachment("orderId");
            var order = await _orderDomainService.GetById(orderId.To<long>());
            order.Status = OrderStatus.Payed;
            order = await _orderDomainService.Update(order);
            return order.MapTo<GetOrderOutput>();
        }

        public async Task OrderCreateCancel(CreateOrderInput input)
        {
            var orderId = RpcContext.GetContext().GetAttachment("orderId");
            if (orderId != null)
            {
                // await _orderDomainService.Delete(orderId.To<long>());
                var order = await _orderDomainService.GetById(orderId.To<long>());
                order.Status = OrderStatus.UnPay;
                await _orderDomainService.Update(order);
            }
        }
    }
}