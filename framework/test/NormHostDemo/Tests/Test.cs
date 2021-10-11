using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Silky.EntityFrameworkCore.Entities;
using Silky.EntityFrameworkCore.Entities.Configures;

namespace NormHostDemo.Tests
{
    public class Test : IEntity, IEntitySeedData<Test>
    {
        [Key] public long Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public IEnumerable<Test> HasData(DbContext dbContext, Type dbContextLocator)
        {
            return new List<Test>()
            {
                new()
                {
                    Id = 1,
                    Name = "test",
                    Address = "beijing"
                },
            };
        }
    }
}