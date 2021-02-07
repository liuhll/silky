using System;
using System.Reflection;
using Lms.Core.Extensions;

namespace Lms.Rpc.Runtime.Server.ServiceEntry.Parameter
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor(ParameterFrom @from, ParameterInfo parameterInfo, string name = null)
        {
            From = @from;
            ParameterInfo = parameterInfo;
            Name = !name.IsNullOrEmpty() ? name : parameterInfo.Name;
            Type = parameterInfo.ParameterType;
        }

        public ParameterFrom From { get; }

        public Type Type { get; }

        public string Name { get; }

        public bool IsSample => Type.IsSample();

        public ParameterInfo ParameterInfo { get; }
    }
}