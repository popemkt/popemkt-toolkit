using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcRespawn;
using Microsoft.Extensions.Hosting;

var app = Host.CreateDefaultBuilder(args)
    #if LINUX
    .UseSystemd()
    #endif
    .ConfigureAppConfiguration(((_, builder) =>
    {
        Console.WriteLine("Configuring appsettings.json");
        var configPath = args.Length > 0
            ? args[0]
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings.{GetOs()}.json");
        Console.WriteLine(configPath);
        builder.AddJsonFile(configPath, optional: false, reloadOnChange: true);
    }))
    .ConfigureServices((context, services) =>
    {
        Console.WriteLine("Configuring Services");
        services.AddOptions();
        services.Configure<AppConfig>(context.Configuration);
        services.AddHostedService<ProcRespawnDaemon>();
        services.Configure<HostOptions>(x =>
        {
            x.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
            x.ServicesStartConcurrently = true;
            x.ServicesStopConcurrently = true;
        });
    })
    .Build();

Console.WriteLine("Running ProcRespawn...");
await app.RunAsync();
Console.WriteLine("ProcRespawn Exited");

static string GetOs()
{
    return Environment.OSVersion.Platform switch
    {
        PlatformID.Unix => "linux",
        PlatformID.Win32NT => "windows",
        _ => throw new NotSupportedException("Unsupported OS")
    };
}