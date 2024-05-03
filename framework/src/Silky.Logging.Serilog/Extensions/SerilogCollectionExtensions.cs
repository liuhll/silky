using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;


namespace Silky.Logging.Serilog.Extensions
{
    public static class SerilogCollectionExtensions
    {
        public static IServiceCollection AddSerilogDefault(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(new ExpressionTemplate(
                    // Include trace and span ids when present.
                    "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}",
                 theme: TemplateTheme.Code)));
            return services;
        }
    }
}
