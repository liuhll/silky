using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silky.Validation.StringValues
{
    public interface IStringValueType
    {
        string Name { get; }

        [CanBeNull] object this[string key] { get; set; }

        [NotNull] Dictionary<string, object> Properties { get; }

        IValueValidator Validator { get; set; }
    }
}