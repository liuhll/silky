using System.Collections.Generic;
using System.Linq;
using System.Net;
using Silky.Core.Extensions;

namespace Silky.Core.Exceptions
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

        public static HttpStatusCode GetHttpStatusCode(this StatusCode statusCode)
        {
            if (statusCode.IsBusinessStatus() || statusCode.IsUserFriendlyStatus())
            {
                return HttpStatusCode.BadRequest;
            }

            HttpStatusCode httpStatusCode = HttpStatusCode.OK;
            switch (statusCode)
            {
                case StatusCode.Success:
                    httpStatusCode = HttpStatusCode.OK;
                    break;
                case StatusCode.UnAuthentication:
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    break;
                case StatusCode.UnAuthorization:
                    httpStatusCode = HttpStatusCode.Forbidden;
                    break;
                case StatusCode.UserFriendly:
                case StatusCode.BusinessError:
                case StatusCode.ValidateError:
                    httpStatusCode = HttpStatusCode.BadRequest;
                    break;
                case StatusCode.Timeout:
                case StatusCode.DeadlineExceeded:
                    httpStatusCode = HttpStatusCode.RequestTimeout;
                    break;
                default:
                    httpStatusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            return httpStatusCode;
        }

        public static bool IsUserFriendlyStatus(this StatusCode statusCode)
        {
            if (statusCode.GetAttribute<IsUserFriendlyExceptionAttribute>() != null)
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

        public static bool IsFriendlyStatus(this StatusCode statusCode)
        {
            return statusCode.IsBusinessStatus() || statusCode.IsUserFriendlyStatus() || statusCode.IsUnauthorized();
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