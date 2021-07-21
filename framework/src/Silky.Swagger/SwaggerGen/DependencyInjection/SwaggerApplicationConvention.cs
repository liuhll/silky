using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Silky.Swagger.SwaggerGen.DependencyInjection
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }
    }
}