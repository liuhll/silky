using System;

namespace Silky.Core.Exceptions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IsUnAuthorizedExceptionAttribute : Attribute
    {
    }
}