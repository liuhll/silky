using System;

namespace Lms.Core.Convertible
{
    public interface ITypeConvertibleService
    {
        object Convert(object instance, Type conversionType);
    }
}