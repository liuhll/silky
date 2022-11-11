using System.ComponentModel.DataAnnotations;
using Silky.Rpc.Runtime.Server;

namespace ITestApplication.Test.Dtos
{
    public class TestInput
    {
       
        [HashKey]
        [Required(ErrorMessage = "名称不允许为空")]
        public string Name { get; set; }

        [Required(ErrorMessage = "地址不允许为空")]
        [CacheKey(1)]
        public string Address { get; set; }
        
        [CacheKey(0)]
        public long? Id { get; set; }
    }
}