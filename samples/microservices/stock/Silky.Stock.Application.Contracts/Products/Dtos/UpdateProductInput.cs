using Silky.Rpc.CachingInterceptor;

namespace Silky.Stock.Application.Contracts.Products.Dtos
{
    public class UpdateProductInput : ProductDtoBase
    {
        [CacheKey(0)]
        public long Id { get; set; }
    }
}