using System.Collections.Generic;
using Silky.Lms.Core;

namespace Silky.Lms.Validation.StringValues
{
    public class StaticSelectionStringValueItemSource : ISelectionStringValueItemSource
    {
        public ICollection<ISelectionStringValueItem> Items { get; }

        public StaticSelectionStringValueItemSource(params ISelectionStringValueItem[] items)
        {
            Items = Check.NotNullOrEmpty(items, nameof(items));
        }
    }
}