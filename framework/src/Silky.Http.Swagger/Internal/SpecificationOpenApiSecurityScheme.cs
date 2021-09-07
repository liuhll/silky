using Microsoft.OpenApi.Models;

namespace Silky.Http.Swagger.Internal
{
    public class SpecificationOpenApiSecurityScheme : OpenApiSecurityScheme
    {
        public SpecificationOpenApiSecurityScheme()
        {
        }

        /// <summary>
        /// 唯一Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 安全需求
        /// </summary>
        public SpecificationOpenApiSecurityRequirementItem Requirement { get; set; }
    }
}