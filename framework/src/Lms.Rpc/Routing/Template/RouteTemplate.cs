using System.Collections.Generic;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace Lms.Rpc.Routing.Template
{
    public class RouteTemplate
    {
        public TemplateSegment AppService { get; }
        
        public TemplateSegment Entry { get; }

        public IEnumerable<TemplatePart> Parameters { get; set; }

        public string Path { get; }

        public HttpMethod HttpMethod { get; }
    }
}