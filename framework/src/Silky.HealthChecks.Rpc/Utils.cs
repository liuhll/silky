using Silky.Core;

namespace Silky.HealthChecks.Rpc;

public static class Utils
{
    public static string GetHealthCheckServiceEntryId()
    {
        if (EngineContext.Current.ApplicationOptions.UsingServiceShortName)
        {
            return HealthCheckConstants.HealthCheckShortServiceEntryId;
        }

        return HealthCheckConstants.HealthCheckServiceEntryId;
    }
}