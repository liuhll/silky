using System.Threading.Tasks;
using ITestApplication.Test.Dtos;

namespace ITestApplication.Test.Fallback
{
    public interface IUpdatePartFallBack
    {
        Task<string> UpdatePart(TestInput input);
    }
}