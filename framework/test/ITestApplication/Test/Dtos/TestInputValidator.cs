using FluentValidation;

namespace ITestApplication.Test.Dtos
{
    public class TestInputValidator : AbstractValidator<TestInput>
    {
        public TestInputValidator()
        {
            RuleFor(x => x.Name).Length(3, 10).WithMessage("姓名长度为3~10位(Fluent)");
            // RuleFor(x => x.Phone).Matches("^1[3-9]\\d{9}$").WithMessage("手机号码格式不正确");
            RuleFor(x => x.Address).NotEmpty().WithMessage("地址不允许为空(Fluent)");
        }
    }
}