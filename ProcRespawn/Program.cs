using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcRespawn;
using Microsoft.Extensions.Hosting;

var app = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(((context, builder) =>
    {
        var configPath = args.Length > 0
            ? args[0]
            : Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");
        Console.WriteLine(configPath);
        builder.AddJsonFile(configPath, optional: false, reloadOnChange: true);
    }))
    .ConfigureServices((context, services) =>
    {
        services.AddOptions();
        services.Configure<AppConfig>(context.Configuration);
        services.AddHostedService<DaemonService>();
    })
    .UseConsoleLifetime()
    .Build();

await app.RunAsync();