using System.Threading.Tasks;
using Lms.Order.Application.Contracts.Orders.Dtos;
using Silky.Lms.Core.DependencyInjection;

namespace Lms.Order.Domain.Orders
{
    public interface IOrderDomainService : ITransientDependency
    {
        Task Delete(long id);

        Task<Order> GetById(long id);
        Task<Order> Update(Order order);
        Task<GetOrderOutput> Create(CreateOrderInput input);
    }
}