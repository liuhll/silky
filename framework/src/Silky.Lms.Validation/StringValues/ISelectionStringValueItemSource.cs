using System.Collections.Generic;

namespace Silky.Lms.Validation.StringValues
{
    public interface ISelectionStringValueItemSource
    {
        ICollection<ISelectionStringValueItem> Items { get; }
    }
}