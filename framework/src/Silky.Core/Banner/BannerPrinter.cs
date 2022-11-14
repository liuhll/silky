using System;
using Microsoft.Extensions.Logging;

namespace Silky.Core;

internal class BannerPrinter : IBannerPrinter
{
    private ILogger<BannerPrinter> _logger;

    public BannerPrinter(ILogger<BannerPrinter> logger)
    {
        _logger = logger;
    }

    public void Print()
    {
        var banner = EngineContext.Current.Banner;
        switch (banner.Mode)
        {
            case BannerMode.LOG:
                _logger.LogInformation("\n" + banner.Content);
                _logger.LogInformation("  :: Version ::              {0}", banner.Version);
                _logger.LogInformation("  :: Docs ::              {0}", banner.DocUrl);
                break;

            case BannerMode.CONSOLE:
                Console.WriteLine(banner.Content);
                Console.WriteLine("  :: Version ::              {0}", banner.Version);
                Console.WriteLine("  :: Docs ::              {0}\n", banner.DocUrl);
                break;
            default:
                break;
        }
    }
}