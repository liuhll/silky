using System.IO;
using Silky.Core.Configuration;
using Silky.Core.Extensions;

namespace Silky.Core;

internal static class BannerHelper
{
    public static Banner BuildBanner(SilkyApplicationCreationOptions options)
    {
        var content = options.BannerContent;
        if (content.IsNullOrEmpty() && File.Exists("banner.txt"))
        {
            content = File.ReadAllText("banner.txt");
        }

        var docUrl = options.DocUrl;
        var bannerMode = options.BannerMode;
        return new Banner(content, docUrl, bannerMode);
    }
}