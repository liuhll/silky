using System;
using System.Linq;
using System.Reflection;
using Lms.Core.Exceptions;
using Lms.Core.Extensions;

namespace Lms.Rpc.Runtime.Server.Parameter
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor(ParameterFrom @from, ParameterInfo parameterInfo, string name = null)
        {
            From = @from;
            ParameterInfo = parameterInfo;
            Name = !name.IsNullOrEmpty() ? name : parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            IsHashKey = DecideIsHashKey();
        }

        private bool DecideIsHashKey()
        {
            var hashKeyProvider = ParameterInfo.GetCustomAttributes().OfType<IHashKeyProvider>();
            if (hashKeyProvider.Any())
            {
                if (IsSample)
                {
                    return true;
                }
            }

            var propsHashKeyProvider = Type.GetProperties()
                .SelectMany(p => p.GetCustomAttributes().OfType<IHashKeyProvider>());
            if (propsHashKeyProvider.Count() > 1)
            {
                throw new LmsException("不允许指定多个HashKey");
            }

            return false;
        }

        public ParameterFrom From { get; }

        public Type Type { get; }

        public bool IsHashKey { get; }

        public string Name { get; }

        public bool IsSample => Type.IsSample();

        public ParameterInfo ParameterInfo { get; }
    }
}