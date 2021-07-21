using System;
using System.Collections.Generic;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Validation.StringValues
{
    [Serializable]
    public abstract class ValueValidatorBase : IValueValidator
    {
        public virtual string Name => ValueValidatorAttribute.GetName(GetType());

        public object this[string key]
        {
            get => Properties.GetOrDefault(key);
            set => Properties[key] = value;
        }

        public IDictionary<string, object> Properties { get; }

        protected ValueValidatorBase()
        {
            Properties = new Dictionary<string, object>();
        }

        public abstract bool IsValid(object value);
    }
}