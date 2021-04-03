using System.Threading.Tasks;
using Lms.AutoMapper;
using Lms.Stock.Application.Contracts.Products;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Stock.Domain.Products;

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
    }
}