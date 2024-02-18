using FreeSql.DataAnnotations;

namespace FreeSqlHostDemo.Blogs;

public class Blog
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    public string Url { get; set; }
    public int Rating { get; set; }
}