using System;

namespace Silky.Validation.StringValues
{
    [Serializable]
    [StringValueType("TOGGLE")]
    public class ToggleStringValueType : StringValueTypeBase
    {
        public ToggleStringValueType()
            : this(new BooleanValueValidator())
        {
        }

        public ToggleStringValueType(IValueValidator validator)
            : base(validator)
        {
        }
    }
}