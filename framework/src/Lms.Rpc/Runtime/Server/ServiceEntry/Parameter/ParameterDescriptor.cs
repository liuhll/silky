using System;
using System.Linq;
using System.Reflection;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public class ParameterDescriptor
    {
        
        public ParameterDescriptor(string name,Type type,ParameterFrom @from)
        {
            Name = name;
            Type = type;
            From = @from;
        }

        public ParameterFrom From { get; private set; }
        
        public Type Type { get; }

        public string Name { get; }
    }
}