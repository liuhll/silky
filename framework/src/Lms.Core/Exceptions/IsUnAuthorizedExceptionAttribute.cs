using System;

namespace Lms.Core.Exceptions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IsUnAuthorizedExceptionAttribute : Attribute
    {
        
    }
}