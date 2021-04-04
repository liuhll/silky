using System.Threading.Tasks;
using Lms.AutoMapper;
using Lms.Core.Exceptions;
using Lms.Rpc.Transport;
using Lms.Stock.Application.Contracts.Products;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Stock.Domain.Products;
using Lms.Transaction.Tcc;

namespace Lms.Stock.Application.Products
{
    public class ProductAppService : IProductAppService
    {
        private readonly IProductDomainService _productDomainService;

        public ProductAppService(IProductDomainService productDomainService)
        {
            _productDomainService = productDomainService;
        }

        public async Task<GetProductOutput> Create(CreateProductInput input)
        {
            var product = input.MapTo<Product>();
            product = await _productDomainService.Create(product);
            return product.MapTo<GetProductOutput>();
        }

        public async Task<GetProductOutput> Update(UpdateProductInput input)
        {
            var product = await _productDomainService.Update(input);
            return product.MapTo<GetProductOutput>();
        }

        public async Task<GetProductOutput> Get(long id)
        {
            var product = await _productDomainService.GetById(id);
            return product.MapTo<GetProductOutput>();
        }

        public Task Delete(long id)
        {
            return _productDomainService.Delete(id);
        }

        [TccTransaction(ConfirmMethod = "DeductStockConfirm", CancelMethod = "DeductStockCancel")]
        public async Task<GetProductOutput> DeductStock(DeductStockInput input)
        {
            var product = await _productDomainService.GetById(input.ProductId);
            if (input.Quantity > product.Stock)
            {
                throw new BusinessException("订单数量超过库存数量,无法完成订单");
            }

            return product.MapTo<GetProductOutput>();
            // todo 真实业务场景中可以考虑先锁定资源,再confim阶段确认库存或是cancel恢复库存
        }

        public async Task<GetProductOutput> DeductStockConfirm(DeductStockInput input)
        {
            var product = await _productDomainService.DeductStock(input);
            return product.MapTo<GetProductOutput>();
        }

        public Task DeductStockCancel(DeductStockInput input)
        {
            return Task.CompletedTask;
        }
    }
}