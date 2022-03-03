using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Abstraction.Factory
{
    public interface ISilkySegmentContextFactory
    {
        SegmentContext GetEntryContext(string serviceEntryId);
        
        SegmentContext GetCurrentEntryContext(string serviceEntryId);

        SegmentContext GetExitContext(string serviceEntryId);
        
        SegmentContext GetCurrentExitContext(string serviceEntryId);

        SegmentContext GetHttpHandleExitContext(string id);

        SegmentContext GetCurrentContext(string operationName);

        SegmentContext GetTransactionContext(string operationName);

        void ReleaseContext(SegmentContext context);
    }
}