using System.Threading.Tasks;
using Silky.Stock.Application.Contracts.Products.Dtos;
using Microsoft.AspNetCore.Mvc;
using Silky.Rpc.Routing;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;
using Silky.Transaction;

namespace Silky.Stock.Application.Contracts.Products
{
    [ServiceRoute]
    [Authorize]
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
        [UpdateCachingIntercept("Product:Id:{Id}")]
        Task<GetProductOutput> Update(UpdateProductInput input);

        /// <summary>
        /// 通过Id获取产品信息
        /// </summary>
        /// <param name="id">产品Id</param>
        /// <returns></returns>
        [GetCachingIntercept("Product:Id:{id}")]
        [HttpGet("{id:long}")]
        Task<GetProductOutput> Get(long id);

        /// <summary>
        /// 通过Id删除产品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [RemoveCachingIntercept("GetProductOutput","Product:Id:{id}")]
        [HttpDelete("{id:long}")]
        Task Delete(long id);

        [Transaction]
        [RemoveCachingIntercept("GetProductOutput","Product:Id:{ProductId}")]
        [Governance(ProhibitExtranet = true)]
        Task<GetProductOutput> DeductStock(DeductStockInput input);
    }
}