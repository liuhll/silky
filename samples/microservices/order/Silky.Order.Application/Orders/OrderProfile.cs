using AutoMapper;
using Silky.Order.Application.Contracts.Orders.Dtos;

namespace Silky.Order.Application.Orders
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<CreateOrderInput, Domain.Orders.Order>();
            CreateMap<Domain.Orders.Order, GetOrderOutput>();
        }
    }
}