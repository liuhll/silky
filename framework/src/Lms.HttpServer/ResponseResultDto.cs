using System.Collections.Generic;
using Lms.Core.Exceptions;

namespace Lms.HttpServer
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