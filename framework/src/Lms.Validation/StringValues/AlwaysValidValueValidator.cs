using System;

namespace Lms.Validation.StringValues
{
    [Serializable]
    [ValueValidator("NULL")]
    public class AlwaysValidValueValidator : ValueValidatorBase
    {
        public override bool IsValid(object value)
        {
            return true;
        }
    }
}