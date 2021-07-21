using System.ComponentModel.DataAnnotations;
using Silky.EntityFrameworkCore.Entities;

namespace NormHostDemo.Tests
{
    public class Test : IEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
    }
}