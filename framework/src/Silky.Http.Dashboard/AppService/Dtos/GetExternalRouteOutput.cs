using System.Collections.Generic;

namespace Silky.Http.Dashboard.AppService.Dtos
{
    public class GetExternalRouteOutput
    {
        public GetExternalRouteOutput()
        {
            Children = new List<GetExternalRouteOutput>();
            Meta = new Dictionary<string, object>();
        }

        public string Path { get; set; }
        public string Name { get; set; }
        public IDictionary<string, object> Meta { get; set; }

        public ICollection<GetExternalRouteOutput> Children { get; set; }
    }
}