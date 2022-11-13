using Silky.Core;

namespace Silky.Stock.Application.Contracts.Products.Dtos
{
    [CacheName("GetProductOutput")]
    public class GetProductOutput : ProductDtoBase
    {
        /// <summary>
        /// 产品Id
        /// </summary>
        public long Id { get; set; }
    }
}