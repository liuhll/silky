using Silky.Core.Utils;

namespace Silky.Core;

internal class Banner
{
    public Banner(string content, string docUrl, BannerMode mode = BannerMode.CONSOLE)
    {
        Content = content ?? @"                                              
   _____  _  _  _           
  / ____|(_)| || |          
 | (___   _ | || | __ _   _ 
  \___ \ | || || |/ /| | | |
  ____) || || ||   < | |_| |
 |_____/ |_||_||_|\_\ \__, |
                       __/ |
                      |___/
            ";

        DocUrl = docUrl ?? "https://docs.silky-fk.com";
        Mode = mode;
        Version = VersionHelper.GetSilkyVersion();
    }

    public string Content { get; private set; }

    public BannerMode Mode { get; private set; }

    public string Version { get; private set; }

    public string DocUrl { get; private set; }
}