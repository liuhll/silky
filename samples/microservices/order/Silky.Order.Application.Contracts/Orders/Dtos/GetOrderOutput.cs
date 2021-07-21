using Silky.Order.Domain.Shared.Orders;

namespace Silky.Order.Application.Contracts.Orders.Dtos
{
    public class GetOrderOutput
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 账号Id
        /// </summary>
        public long AccountId { get; set; }
        
        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus Status { get; set; }
    }
}