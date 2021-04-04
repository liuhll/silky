using System.Threading.Tasks;
using Lms.Core.DependencyInjection;

namespace Lms.Order.Domain.Orders
{
    public interface IOrderDomainService : ITransientDependency
    {
        Task<Order> Create(Order order);
        Task Delete(long id);

        Task<Order> GetById(long id);
        Task<Order> Update(Order order);
    }
}