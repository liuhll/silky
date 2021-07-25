using System.Collections.Generic;
using Silky.Core.Exceptions;

namespace Silky.Http.Core
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