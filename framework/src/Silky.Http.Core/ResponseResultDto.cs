using System.Collections.Generic;
using Silky.Core.Exceptions;

namespace Silky.Http.Core
{
    public class ResponseResultDto
    {
        public object Result { get; set; }

        public int Status { get; set; }

        public string Code { get; set; }

        public string ErrorMessage { get; set; }

        public IEnumerable<ValidError> ValidErrors { get; set; }
    }
}