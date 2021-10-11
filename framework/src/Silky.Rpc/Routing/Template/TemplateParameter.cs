namespace Silky.Rpc.Routing.Template
{
    public class TemplateParameter
    {
        public TemplateParameter(string name)
        {
            Name = name;
        }

        public TemplateParameter(string name, string constraint)
        {
            Name = name;
            Constraint = constraint;
        }

        public string Name { get; }

        public string Constraint { get; }
    }
}