using System.Threading.Tasks;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Runtime.Server.ServiceDiscovery;
using Lms.Rpc.Transport.CachingIntercept;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace Lms.Stock.Application.Contracts.Products
{
    [ServiceRoute]
    public interface IProductAppService
    {
        /// <summary>
        /// 新增产品
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<GetProductOutput> Create(CreateProductInput input);

        /// <summary>
        /// 更新产品
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [UpdateCachingIntercept("Product:Id:{0}")]
        Task<GetProductOutput> Update(UpdateProductInput input);

        /// <summary>
        /// 通过Id获取产品信息
        /// </summary>
        /// <param name="id">产品Id</param>
        /// <returns></returns>
        [GetCachingIntercept("Product:Id:{0}")]
        [HttpGet("{id:long}")]
        Task<GetProductOutput> Get([CacheKey(0)]long id);

        /// <summary>
        /// 通过Id删除产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RemoveCachingIntercept("GetProductOutput","Product:Id:{0}")]
        [HttpDelete("{id:long}")]
        Task Delete([CacheKey(0)]long id);

        [Transaction]
        [RemoveCachingIntercept("GetProductOutput","Product:Id:{0}")]
        [Governance(ProhibitExtranet = true)]
        Task<GetProductOutput> DeductStock(DeductStockInput input);
    }
}