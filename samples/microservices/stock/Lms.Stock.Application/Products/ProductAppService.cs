using System.Threading.Tasks;
using Lms.Stock.Application.Contracts.Products;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Stock.Domain.Products;
using Silky.Lms.AutoMapper;
using Silky.Lms.Core.Exceptions;
using Silky.Lms.Transaction.Tcc;

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

            product.LockStock += input.Quantity;
            product.Stock -= input.Quantity;
            product = await _productDomainService.Update(product);
            return product.MapTo<GetProductOutput>();
          
        }

        public async Task<GetProductOutput> DeductStockConfirm(DeductStockInput input)
        {
            var product = await _productDomainService.DeductStockConfirm(input);
            return product.MapTo<GetProductOutput>();
        }

        public Task DeductStockCancel(DeductStockInput input)
        {
             return _productDomainService.DeductStockCancel(input);
           
        }
    }
}