using System.Collections.Generic;
using System.Linq;
using Lms.Core.Extensions;

namespace Lms.Core.Exceptions
{
    public static class StatusCodeExtensions
    {
        public static bool IsResponseStatus(this StatusCode statusCode)
        {
            if (statusCode.GetAttribute<IsResponseStatusAttribute>() != null)
            {
                return true;
            }

            return false;
        }
    }


    public static class StatusCodeHelper
    {
        public static IDictionary<StatusCode,string> GetResponseStatusCodes()
        {
            var statusCodes = typeof(StatusCode).GetEnumSources<StatusCode>()
                .Where(p => p.Key.IsResponseStatus());
            return statusCodes.ToDictionary(p=> p.Key,p=> p.Value);
        }
    }


}
    
    
