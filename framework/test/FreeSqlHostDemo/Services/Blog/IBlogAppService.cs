using FreeSqlHostDemo.Services.Blog.Dtos;
using Silky.Rpc.Routing;
using Silky.Rpc.Security;

namespace FreeSqlHostDemo.Services.Blog;

[ServiceRoute]
[AllowAnonymous]
public interface IBlogAppService
{
    Task Create(CreateBlogInput input);
}