using System.Threading.Tasks;
using Lms.Order.Application.Contracts.Orders.Dtos;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Transaction;

namespace Lms.Order.Application.Contracts.Orders
{
    [ServiceRoute]
    public interface IOrderAppService
    {
        /// <summary>
        /// 新增订单接口
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Transaction]
        Task<GetOrderOutput> Create(CreateOrderInput input);
    }
}