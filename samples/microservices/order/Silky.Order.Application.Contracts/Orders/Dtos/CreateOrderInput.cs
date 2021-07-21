using System.ComponentModel.DataAnnotations;

namespace Silky.Order.Application.Contracts.Orders.Dtos
{
    public class CreateOrderInput
    {
        /// <summary>
        /// 账号Id
        /// </summary>
        [Required(ErrorMessage = "账号Id不允许为空")]
        public long AccountId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        [Required(ErrorMessage = "产品Id不允许为空")]
        public long ProductId { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        [Required(ErrorMessage = "产品数量不允许为空")]
        public int Quantity { get; set; }
    }
}