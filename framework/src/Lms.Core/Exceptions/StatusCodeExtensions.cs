using System.Collections.Generic;
using System.Linq;
using Lms.Core.Extensions;

namespace Lms.Core.Exceptions
{
    public static class StatusCodeExtensions
    {
        public static bool IsBusinessStatus(this StatusCode statusCode)
        {
            if (statusCode.GetAttribute<IsBusinessExceptionAttribute>() != null)
            {
                return true;
            }

            return false;
        }

        public static bool IsUnauthorized(this StatusCode statusCode)
        {
            if (statusCode.GetAttribute<IsUnAuthorizedExceptionAttribute>() != null)
            {
                return true;
            }

            return false;
        }
    }


    public static class StatusCodeHelper
    {
        public static IDictionary<StatusCode, string> GetAllStatusCodes()
        {
            var statusCodes = typeof(StatusCode).GetEnumSources<StatusCode>();
            return statusCodes.ToDictionary(p => p.Key, p => p.Value);
        }
    }
}