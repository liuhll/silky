using System.ComponentModel.DataAnnotations;

namespace ITestApplication.Test.Dtos
{
    public class TestDto
    {
        [Required(ErrorMessage = "姓名不允许为空")]
        public string Name { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }
    }
}