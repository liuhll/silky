using System;

namespace Silky.Swagger.Abstraction.SwaggerGen.SwaggerGenerator
{
    public class SwaggerGeneratorException : Exception
    {
        public SwaggerGeneratorException(string message) : base(message)
        {
        }

        public SwaggerGeneratorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}