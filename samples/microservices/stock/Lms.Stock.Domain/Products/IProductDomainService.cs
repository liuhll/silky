using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Stock.Application.Contracts.Products.Dtos;

namespace Lms.Stock.Domain.Products
{
    public interface IProductDomainService : ITransientDependency
    {
        Task<Product> Create(Product product);
        Task<Product> Update(UpdateProductInput input);

        Task<Product> GetById(long id);
        Task Delete(long id);
    }
}