using AutoMapper;
using Lms.Order.Application.Contracts.Orders.Dtos;

namespace Lms.Order.Application.Orders
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