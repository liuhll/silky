using System.ComponentModel.DataAnnotations;

namespace Silky.Stock.Application.Contracts.Products.Dtos
{
    public abstract class ProductDtoBase
    {
        /// <summary>
        /// 产品名称
        /// </summary>
        [Required(ErrorMessage = "产品名称不允许为空")]
        [MaxLength(100,ErrorMessage = "产品名称不允许超过100个字符")]
        [MinLength(2, ErrorMessage = "产品名称不允许小于2个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        [Required(ErrorMessage = "单价不允许为空")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        [Required(ErrorMessage = "库存数量不允许为空")]
        public int Stock { get; set; } = 0;
    }
}