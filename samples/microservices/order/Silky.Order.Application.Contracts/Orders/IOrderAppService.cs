using System.Threading.Tasks;
using Silky.Order.Application.Contracts.Orders.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;
using Silky.Transaction;

namespace Silky.Order.Application.Contracts.Orders
{
    [ServiceRoute]
    [Authorize]
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