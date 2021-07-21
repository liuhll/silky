using System.Threading.Tasks;
using Silky.Order.Application.Contracts.Orders.Dtos;
using Silky.Core.DependencyInjection;

namespace Silky.Order.Domain.Orders
{
    public interface IOrderDomainService : ITransientDependency
    {
        Task Delete(long id);

        Task<Order> GetById(long id);
        Task<Order> Update(Order order);
        Task<GetOrderOutput> Create(CreateOrderInput input);
    }
}