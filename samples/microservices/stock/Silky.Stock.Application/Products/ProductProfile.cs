using System;
using AutoMapper;
using Silky.Stock.Application.Contracts.Products.Dtos;
using Silky.Stock.Domain.Products;
using Silky.Rpc.Runtime.Session;

namespace Silky.Stock.Application.Products
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