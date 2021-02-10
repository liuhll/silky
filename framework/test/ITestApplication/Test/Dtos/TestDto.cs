using System.ComponentModel.DataAnnotations;
using Lms.Rpc.Runtime.Server.Parameter;

namespace ITestApplication.Test.Dtos
{
    public class TestDto
    {
      
        public string Name { get; set; }

        [Required(ErrorMessage = "地址不允许为空")]
        [HashKey]
        public string Address { get; set; }

        public string Phone { get; set; }
    }
}