using System.ComponentModel.DataAnnotations;

namespace NormHostDemo.Tests
{
    public class Test
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
    }
}