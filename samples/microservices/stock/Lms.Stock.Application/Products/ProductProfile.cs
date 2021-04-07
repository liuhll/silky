using System;
using AutoMapper;
using Lms.Stock.Application.Contracts.Products.Dtos;
using Lms.Stock.Domain.Products;
using Silky.Lms.Rpc.Runtime.Session;

namespace Lms.Stock.Application.Products
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<CreateProductInput, Product>();
            CreateMap<Product, GetProductOutput>();
            CreateMap<UpdateProductInput, Product>().AfterMap((src, dest) =>
            {
                dest.UpdateBy = NullSession.Instance.UserId;
                dest.UpdateTime = DateTime.Now;
            });
            
        }
    }
}