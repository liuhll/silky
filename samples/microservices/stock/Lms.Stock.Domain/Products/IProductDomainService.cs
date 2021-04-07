using System.Threading.Tasks;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Silky.Lms.Core.DependencyInjection;

namespace Lms.Stock.Domain.Products
{
    public interface IProductDomainService : ITransientDependency
    {
        Task<Product> Create(Product product);
        Task<Product> Update(UpdateProductInput input);

        Task<Product> GetById(long id);
        Task Delete(long id);
        Task<Product> DeductStock(DeductStockInput input);
    }
}