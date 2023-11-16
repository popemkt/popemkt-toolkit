using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcRespawn;
using Microsoft.Extensions.Hosting;

var app = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureAppConfiguration(((context, builder) =>
    {
        Console.WriteLine("Configuring appsettings.json");
        var configPath = args.Length > 0
            ? args[0]
            : Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        Console.WriteLine(configPath);
        builder.AddJsonFile(configPath, optional: false, reloadOnChange: true);
    }))
    .ConfigureServices((context, services) =>
    {
        Console.WriteLine("Configuring Services");
        services.AddOptions();
        services.Configure<AppConfig>(context.Configuration);
        services.AddHostedService<DaemonService>();
    })
    .Build();

Console.WriteLine("Running ProcRespawn...");
await app.RunAsync();
Console.WriteLine("ProcRespawn Exited");