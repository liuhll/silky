using System.Threading.Tasks;
using Silky.Rpc.Filters;

namespace ITestApplication.Test.Filters;

public class TestServerFilterAttribute : ServerFilterAttribute
{
    public override async Task OnResultExecutionAsync(ServerResultExecutingContext context, ServerResultExecutionDelegate next)
    {
        await next();
    }
}