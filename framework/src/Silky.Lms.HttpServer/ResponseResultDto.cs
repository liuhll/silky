using System.Collections.Generic;
using Silky.Lms.Core.Exceptions;

namespace Silky.Lms.HttpServer
{
    public class ResponseResultDto
    {
        public object Data { get; set; }

        public StatusCode Status { get; set; }

        public string StatusCode => Status.ToString();

        public string ErrorMessage { get; set; }

        public IEnumerable<ValidError> ValidErrors { get; set; }
    }
}