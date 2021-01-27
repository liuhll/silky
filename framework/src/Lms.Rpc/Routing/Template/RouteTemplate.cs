using System.Collections.Generic;

namespace Lms.Rpc.Routing.Template
{
    public class RouteTemplate
    {
        public IList<TemplateSegment> Segments { get; set; } = new List<TemplateSegment>();

        public IList<TemplateParameter> Parameters { get; set; } = new List<TemplateParameter>();

    }
}