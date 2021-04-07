using Silky.Lms.Rpc.Transport.CachingIntercept;

namespace Lms.Stock.Application.Contracts.Products.Dtos
{
    public class UpdateProductInput : ProductDtoBase
    {
        [CacheKey(0)]
        public long Id { get; set; }
    }
}