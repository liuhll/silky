using System;

namespace Lms.Rpc.Runtime.Server
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ServiceKeyAttribute : Attribute
    {
        public ServiceKeyAttribute(string name, int weight)
        {
            Name = name;
            Weight = weight;
        }

        public string Name { get; }

        public int Weight { get; }
    }
}