using SkyApm.Tracing.Segments;

namespace Silky.SkyApm.Diagnostics.Abstraction.Factory
{
    public interface ISilkySegmentContextFactory
    {
        SegmentContext GetEntryContext(string serviceEntryId);

        SegmentContext GetExitContext(string serviceEntryId);

        SegmentContext GetCurrentContext(string operationName);

        void ReleaseContext(SegmentContext context);
    }
}