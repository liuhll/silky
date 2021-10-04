using System;
using System.Threading.Tasks;
using Mapster;
using Silky.Core.DbContext.UnitOfWork;
using Silky.Order.Application.Contracts.Orders;
using Silky.Order.Application.Contracts.Orders.Dtos;
using Silky.Order.Domain.Orders;
using Silky.Order.Domain.Shared.Orders;
using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Transaction.Tcc;

namespace Silky.Order.Application.Orders
{
    public class OrderAppService : IOrderAppService
    {
        private readonly IOrderDomainService _orderDomainService;

        public OrderAppService(IOrderDomainService orderDomainService)
        {
            _orderDomainService = orderDomainService;

        }

        [TccTransaction(ConfirmMethod = "OrderCreateConfirm", CancelMethod = "OrderCreateCancel")]
        [UnitOfWork]
        public async Task<GetOrderOutput> Create(CreateOrderInput input)
        {
            var orderOutput = await _orderDomainService.Create(input);
            return orderOutput;
        }

        [UnitOfWork]
        public async Task<GetOrderOutput> OrderCreateConfirm(CreateOrderInput input)
        {
            var orderId = RpcContext.Context.GetAttachment("orderId");
            var order = await _orderDomainService.GetById(orderId.To<long>());
            order.Status = OrderStatus.Payed;
            order.UpdateTime = DateTime.Now;
            order = await _orderDomainService.Update(order);
            return order.Adapt<GetOrderOutput>();
        }
        
        [UnitOfWork]
        public async Task OrderCreateCancel(CreateOrderInput input)
        {
            var orderId = RpcContext.Context.GetAttachment("orderId");
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