using Silky.Transaction.Abstraction;
using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Rpc.Factory
{
    public interface ISilkyRpcSegmentContextFactory
    {
        SegmentContext GetEntryContext(string serviceEntryId);

        SegmentContext GetExitSContext(string serviceEntryId);

        SegmentContext GetTransContext(TransactionRole role, ActionStage action);

        void ReleaseContext(SegmentContext context);
    }
}