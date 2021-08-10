using System.Threading.Tasks;
using Mapster;
using Silky.Account.Application.Contracts.Accounts;
using Silky.Account.Application.Contracts.Accounts.Dtos;
using Silky.Order.Application.Contracts.Orders.Dtos;
using Silky.Stock.Application.Contracts.Products;
using Silky.Stock.Application.Contracts.Products.Dtos;
using Silky.Core.Exceptions;
using Silky.Core.Rpc;
using Silky.EntityFrameworkCore.Repositories;

namespace Silky.Order.Domain.Orders
{
    public class OrderDomainService : IOrderDomainService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IProductAppService _productAppService;
        private readonly IAccountAppService _accountAppService;

        public OrderDomainService(IRepository<Order> orderRepository,
            IProductAppService productAppService, 
            IAccountAppService accountAppService)
        {
            _orderRepository = orderRepository;
            _productAppService = productAppService;
            _accountAppService = accountAppService;
        }

        public async Task<Order> Create(Order order)
        {
            await _orderRepository.InsertNowAsync(order);
            return order;
        }


        public async Task Delete(long id)
        {
            var order = await GetById(id);
            await _orderRepository.DeleteAsync(order);
        }

        public async Task<Order> GetById(long id)
        {
            var order = await _orderRepository.FindAsync(id);
            if (order == null)
            {
                throw new BusinessException($"$不存在ID为{id}的订单");
            }

            return order;
        }

        public async Task<Order> Update(Order order)
        {
            await _orderRepository.UpdateAsync(order);
            return order;
        }
        
        public async Task<GetOrderOutput> Create(CreateOrderInput input)
        {
            // 扣减库存
            var product = await _productAppService.DeductStock(new DeductStockInput()
            {
                Quantity = input.Quantity,
                ProductId = input.ProductId
            });

            // 创建订单
            var order = input.Adapt<Domain.Orders.Order>();
            order.Amount = product.UnitPrice * input.Quantity;
            order = await Create(order);
            RpcContext.Context.SetAttachment("orderId", order.Id);

            //扣减账户余额
            var deductBalanceInput = new DeductBalanceInput()
                {OrderId = order.Id, AccountId = input.AccountId, OrderBalance = order.Amount};
            var orderBalanceId = await _accountAppService.DeductBalance(deductBalanceInput);
            if (orderBalanceId.HasValue)
            {
                RpcContext.Context.SetAttachment("orderBalanceId", orderBalanceId.Value);
            }

            return order.Adapt<GetOrderOutput>();
        }
    }
}