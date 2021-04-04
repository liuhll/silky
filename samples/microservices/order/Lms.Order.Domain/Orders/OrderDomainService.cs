using System.Threading.Tasks;
using Lms.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using TanvirArjel.EFCore.GenericRepository;

namespace Lms.Order.Domain.Orders
{
    public class OrderDomainService : IOrderDomainService
    {
        private readonly IRepository _repository;

        public OrderDomainService(IRepository repository)
        {
            _repository = repository;
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
    }
}