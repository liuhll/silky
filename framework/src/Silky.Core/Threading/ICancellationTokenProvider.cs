using System.Threading;

namespace Silky.Core.Threading
{
    public interface ICancellationTokenProvider
    {
        CancellationToken Token { get; }
    }
}