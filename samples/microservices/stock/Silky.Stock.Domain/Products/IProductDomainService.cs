using System.Threading.Tasks;
using Silky.Stock.Application.Contracts.Products.Dtos;
using Silky.Core.DependencyInjection;

namespace Silky.Stock.Domain.Products
{
    public interface IProductDomainService : ITransientDependency
    {
        Task<Product> Create(Product product);
        Task<Product> Update(UpdateProductInput input);
        Task<Product> Update(Product product);
        

        Task<Product> GetById(long id);
        Task Delete(long id);
        Task<Product> DeductStockConfirm(DeductStockInput input);
        Task DeductStockCancel(DeductStockInput input);
    }
}