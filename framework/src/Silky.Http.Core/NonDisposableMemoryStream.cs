using System.IO;

namespace Silky.Http.Core;

internal sealed class NonDisposableMemoryStream : MemoryStream
{
    protected override void Dispose(bool disposing)
    {
        // Ignore dispose from wrapping compression stream.
        // If MemoryStream is disposed then Length isn't available.
    }
}