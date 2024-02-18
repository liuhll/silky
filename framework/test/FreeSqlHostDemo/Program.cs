// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Hosting;
using Silky.Core;

await CreateHostBuilder(args).Build().RunAsync();

IHostBuilder CreateHostBuilder(string[] args)
{
    var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .ConfigureSilkyGeneralHostDefaults(options =>
            {
                options.DisplayFullErrorStack = true;
                options.ApplicationName = "FreeSqlHostDemo";
                options.BannerMode = BannerMode.CONSOLE;
#if DEBUG
                options.IgnoreCheckingRegisterType = true;
#endif
            })
            .UseSerilogDefault()
        ;
    if (hostBuilder.IsEnvironment("Apollo"))
    {
        hostBuilder.AddApollo();
    }

    return hostBuilder;
}

