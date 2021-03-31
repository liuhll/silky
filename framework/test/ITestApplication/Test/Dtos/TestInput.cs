using System.ComponentModel.DataAnnotations;
using Lms.Rpc.Runtime.Server.Parameter;
using Lms.Rpc.Transport.CachingIntercept;

namespace ITestApplication.Test.Dtos
{
    public class TestInput
    {
        [CacheKey(0)] 
        [HashKey] 
        //[Required(ErrorMessage = "名称不允许为空")]
        public string Name { get; set; }

        //[Required(ErrorMessage = "地址不允许为空")]
        [CacheKey(1)]
        public string Address { get; set; }

        //[Phone(ErrorMessage = "手机号码格式不正确")] 
        public string Phone { get; set; }
    }
}