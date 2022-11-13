
namespace Silky.Stock.Application.Contracts.Products.Dtos
{
    public class DeductStockInput
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
    }
}