using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Silky.Core.Extensions;
using Silky.EntityFrameworkCore.Entities;
using Silky.EntityFrameworkCore.Entities.Configures;
using Silky.Rpc.Runtime.Server;

namespace Silky.Stock.Domain.Products
{
    public class Product : IEntity, IEntitySeedData<Product>
    {
        private readonly ISession _session;
        public Product()
        {
            _session = NullSession.Instance;
            CreateTime = DateTime.Now;
            CreateBy = _session.UserId?.ConventTo<long>();
        }

        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public int LockStock { get; set; }

        public DateTime CreateTime { get; set; }
        
        public long? CreateBy { get; set; }
        
        public DateTime UpdateTime { get; set; }

        public long? UpdateBy { get; set; }
        public IEnumerable<Product> HasData(DbContext dbContext, Type dbContextLocator)
        {
            return new List<Product>()
            {
                new ()
                {
                    Id = 1,
                    Name = "iPhone11",
                    Stock = 100,
                    UnitPrice = 10,
                    CreateTime = DateTime.Now
                },
                new ()
                {
                    Id = 2,
                    Name = "huawei",
                    Stock = 200,
                    UnitPrice = 120,
                    CreateTime = DateTime.Now
                },
                new ()
                {
                    Id = 3,
                    Name = "xiaomi",
                    Stock = 150,
                    UnitPrice = 50,
                    CreateTime = DateTime.Now
                }
            };
        }
    }
}