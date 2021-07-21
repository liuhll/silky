using System.Collections.Generic;

namespace Silky.Validation.StringValues
{
    public interface ISelectionStringValueItemSource
    {
        ICollection<ISelectionStringValueItem> Items { get; }
    }
}