using Silky.Rpc.Runtime.Server;

namespace IAnotherApplication.Dtos;

public class TestDto
{
    [CacheKey(0)]
    public string Name { get; set; }

    public string Address { get; set; }
}