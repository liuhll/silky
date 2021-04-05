using System.Threading.Tasks;
using Lms.Account.Application.Contracts.Accounts;
using Lms.Account.Application.Contracts.Accounts.Dtos;
using Lms.AutoMapper;
using Lms.Core.Exceptions;
using Lms.Order.Application.Contracts.Orders.Dtos;
using Lms.Rpc.Transport;
using Lms.Stock.Application.Contracts.Products;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using TanvirArjel.EFCore.GenericRepository;

namespace Lms.Order.Domain.Orders
{
    public class OrderDomainService : IOrderDomainService
    {
        private readonly IRepository _repository;
        private readonly IProductAppService _productAppService;
        private readonly IAccountAppService _accountAppService;

        public OrderDomainService(IRepository repository,
            IProductAppService productAppService, 
            IAccountAppService accountAppService)
        {
            _repository = repository;
            _productAppService = productAppService;
            _accountAppService = accountAppService;
        }

        public async Task<Order> Create(Order order)
        {
            await _repository.InsertAsync(order);
            return order;
        }


        public async Task Delete(long id)
        {
            var order = await GetById(id);
            await _repository.DeleteAsync(order);
        }

        public async Task<Order> GetById(long id)
        {
            var order = await _repository.GetQueryable<Order>().FirstOrDefaultAsync(p=> p.Id == id);
            if (order == null)
            {
                throw new BusinessException($"$不存在ID为{id}的订单");
            }

            return order;
        }

        public async Task<Order> Update(Order order)
        {
            await _repository.UpdateAsync(order);
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
            var order = input.MapTo<Domain.Orders.Order>();
            order.Amount = product.UnitPrice * input.Quantity;
            order = await Create(order);
            RpcContext.GetContext().SetAttachment("orderId", order.Id);

            //扣减账户余额
            var deductBalanceInput = new DeductBalanceInput()
                {OrderId = order.Id, AccountId = input.AccountId, OrderBalance = order.Amount};
            var orderBalanceId = await _accountAppService.DeductBalance(deductBalanceInput);
            if (orderBalanceId.HasValue)
            {
                RpcContext.GetContext().SetAttachment("orderBalanceId", orderBalanceId.Value);
            }

            return order.MapTo<GetOrderOutput>();
        }
    }
}