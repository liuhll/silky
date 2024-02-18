using FreeSqlHostDemo.Services.Blog.Dtos;
using Silky.ObjectMapper.AutoMapper;

namespace FreeSqlHostDemo.Services.Blog;

public class BlogAppService : IBlogAppService
{
    private readonly IFreeSql _fsql;

    public BlogAppService(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task Create(CreateBlogInput input)
    {
        // var blog = input.Adapt<Blogs.Blog>();
        var blog = new Blogs.Blog()
        {
            Url = input.Url,
            Rating = input.Rating,
        };
       await  _fsql.Insert(blog).ExecuteAffrowsAsync();
    }
}