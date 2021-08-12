using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;

namespace Silky.Rpc.Routing.Template
{
    public class RouteTemplate
    {
        public IList<TemplateSegment> Segments { get; set; } = new List<TemplateSegment>();

        public IList<TemplateParameter> Parameters { get; set; } = new List<TemplateParameter>();

        public int Order
        {
            get
            {
                var order = 1000;
                if (Parameters.Any(p => "int".Equals(p.Constraint) ||
                                        "uint".Equals(p.Constraint) ||
                                        "long".Equals(p.Constraint) ||
                                        "ulong".Equals(p.Constraint) ||
                                        "short".Equals(p.Constraint) ||
                                        "ushort".Equals(p.Constraint)))
                {
                    order = 999;
                }

                if (Parameters.Any(p => "bool".Equals(p.Constraint) ||
                                        "boolean".Equals(p.Constraint)
                ))
                {
                    order = 998;
                }

                if (Parameters.Any(p => "decimal".Equals(p.Constraint) ||
                                        "float".Equals(p.Constraint) ||
                                        "double".Equals(p.Constraint)
                ))
                {
                    order = 997;
                }

                if (Parameters.Any(p => !p.Constraint.IsNullOrEmpty() &&
                                        !"string".Equals(p.Constraint)))
                {
                    order = 996;
                }

                if (Parameters.Any(p => "string".Equals(p.Constraint)
                ))
                {
                    order = 995;
                }

                return order;
            }
        }
    }
}