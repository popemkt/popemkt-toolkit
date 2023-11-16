using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcRespawn;
using Microsoft.Extensions.Hosting;

var app = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(((context, builder) =>
    {
        var configPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        builder.AddJsonFile(configPath);
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
