using System.Threading.Tasks;
using Mapster;
using Silky.Stock.Application.Contracts.Products.Dtos;
using Silky.Core.Exceptions;
using Silky.EntityFrameworkCore.Repositories;

namespace Silky.Stock.Domain.Products
{
    public class ProductDomainService : IProductDomainService
    {
        private readonly IRepository<Product> _productRepository;

        public ProductDomainService(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> Create(Product product)
        {
            var existProduct = _productRepository.FirstOrDefault(p => p.Name == product.Name);
            if (existProduct != null)
            {
                throw new BusinessException($"系统中已经存在{product.Name}的产品");
            }

            await _productRepository.InsertNowAsync(product);
            return product;
        }

        public async Task<Product> Update(UpdateProductInput input)
        {
            var product = await GetById(input.Id);
            product = input.Adapt(product);
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task<Product> Update(Product product)
        {
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public Task<Product> GetById(long id)
        {
            var product = _productRepository.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                throw new BusinessException($"不存在Id为{id}的产品信息");
            }

            return Task.FromResult(product);
        }

        public async Task Delete(long id)
        {
            var account = await GetById(id);
            await _productRepository.DeleteAsync(account);
        }

        public async Task<Product> DeductStockConfirm(DeductStockInput input)
        {
            var product = await GetById(input.ProductId);
            product.LockStock -= input.Quantity;
            await _productRepository.UpdateAsync(product);
            return product;
        }

        public async Task DeductStockCancel(DeductStockInput input)
        {
            var product = await GetById(input.ProductId);
            product.LockStock -= input.Quantity;
            product.Stock += input.Quantity;
            await _productRepository.UpdateAsync(product);
        }
    }
}