using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Extensions;

namespace FreeSqlHostDemo;

public class FreesqlHostConfigureService : IConfigureService
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSilkySkyApm();
        services.AddJwt();
        services.AddObjectMapper();

        Func<IServiceProvider, IFreeSql> fsqlFactory = r =>
        {
            var connectionString = configuration.GetConnectionString("default");
            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, connectionString)
                .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}")) //监听SQL语句
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
                .Build();
            return fsql;
        };

        services.AddSingleton<IFreeSql>(fsqlFactory);

        // services.AddMassTransit(x =>
        // {
        //     x.UsingRabbitMq((context, configurator) =>
        //     {
        //         configurator.Host(configuration["rabbitMq:host"],
        //             configuration["rabbitMq:port"].To<ushort>(),
        //             configuration["rabbitMq:virtualHost"],
        //             config =>
        //             {
        //                 config.Username(configuration["rabbitMq:userName"]);
        //                 config.Password(configuration["rabbitMq:password"]);
        //             });
        //     });
        // });
    }
}