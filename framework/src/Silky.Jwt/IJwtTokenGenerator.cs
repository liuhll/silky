using System.Collections.Generic;

namespace Silky.Jwt
{
    public interface IJwtTokenGenerator
    {
        string Generate(IDictionary<string, object> payload);
    }
}