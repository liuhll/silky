using System.ComponentModel.DataAnnotations;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Transport.CachingIntercept;

namespace ITestApplication.Test.Dtos
{
    public class TestDto
    {
        [CacheKey(1)] public string Name { get; set; }

        [Required(ErrorMessage = "地址不允许为空")]
        [HashKey]
        public string Address { get; set; }

        public string Phone { get; set; }
    }
}