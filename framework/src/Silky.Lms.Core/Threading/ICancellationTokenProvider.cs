using System.Threading;

namespace Silky.Lms.Core.Threading
{
    public interface ICancellationTokenProvider
    {
        CancellationToken Token { get; }
    }
}