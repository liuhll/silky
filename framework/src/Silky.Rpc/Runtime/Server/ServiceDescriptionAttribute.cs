using System;

namespace Silky.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public class ServiceDescriptionAttribute : Attribute
    {
        public string Name { get; }

        public ServiceDescriptionAttribute(string name)
        {
            Name = name;
        }

        public string Description { get; set; }
    }
}