using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Lms.Core.DynamicProxy;
using Lms.Core.Extensions;

namespace Lms.Castle.Adapter
{
    public abstract class CastleLmsMethodInvocationAdapterBase : ILmsMethodInvocation
    {
        public object[] Arguments => Invocation.Arguments;

        public IReadOnlyDictionary<string, object> ArgumentsDictionary => _lazyArgumentsDictionary.Value;
        private readonly Lazy<IReadOnlyDictionary<string, object>> _lazyArgumentsDictionary;

        public Type[] GenericArguments => Invocation.GenericArguments;

        public object TargetObject => Invocation.InvocationTarget ?? Invocation.MethodInvocationTarget;

        public MethodInfo Method => Invocation.MethodInvocationTarget ?? Invocation.Method;

        public bool IsAsyncMethod => Method.IsAsyncMethodInfo();
        
        public object ReturnValue { get; set; }

        protected IInvocation Invocation { get; }

        protected CastleLmsMethodInvocationAdapterBase(IInvocation invocation)
        {
            Invocation = invocation;
            _lazyArgumentsDictionary = new Lazy<IReadOnlyDictionary<string, object>>(GetArgumentsDictionary);
        }

        public abstract Task ProceedAsync();

        private IReadOnlyDictionary<string, object> GetArgumentsDictionary()
        {
            var dict = new Dictionary<string, object>();

            var methodParameters = Method.GetParameters();
            for (int i = 0; i < methodParameters.Length; i++)
            {
                dict[methodParameters[i].Name] = Invocation.Arguments[i];
            }

            return dict;
        }
    }
}