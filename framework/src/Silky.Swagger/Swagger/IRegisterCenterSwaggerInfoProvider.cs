using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace Silky.Swagger;

public interface IRegisterCenterSwaggerInfoProvider
{
    Task<OpenApiDocument> GetSwagger(
        string documentName);

    Task<IEnumerable<OpenApiDocument>> GetSwaggers();
}