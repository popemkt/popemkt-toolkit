using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ProcRespawn;

sealed class DaemonService : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<AppConfig> _configMonitor;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _sts;
    private Task? _task;

    public DaemonService(IOptionsMonitor<AppConfig> configMonitor)
    {
        _sts = new CancellationTokenSource();
        _cancellationToken = _sts.Token;
        _configMonitor = configMonitor;
        // Register a callback to handle configuration changes
        configMonitor.OnChange(_ =>
        {
            Console.WriteLine("Configuration changed. Reloading...");
            // Reload the configuration
            // _configMonitor.CurrentValue = newConfig;
        });
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Starting loop...");
            var config = _configMonitor.CurrentValue;

            foreach (var processConfig in config.Processes)
            {
                var process = FindProcessByName(processConfig.Name);

                if (process is not null)
                {
                    Console.WriteLine($"Process found: {processConfig.Name}");
                }
                else if (process is null || process?.HasExited is true)
                {
                    Console.WriteLine($"Restarting process: {processConfig.Name}");

                    switch (processConfig.Type)
                    {
                        case ExecutableType.Binary:
                            await RestartBinaryExecutableAsync(processConfig.Path);
                            break;
                        case ExecutableType.Desktop:
                            await StartDesktopFileAsync(processConfig.Path);
                            break;
                        default:
                            Console.WriteLine($"Unknown process type: {processConfig.Type}");
                            break;
                    }
                }
            }

            Console.WriteLine("Waiting for next interval...");
            await Task.Delay(_configMonitor.CurrentValue.IntervalInMilliseconds,
                cancellationToken); // Adjust the interval as needed
            Console.WriteLine("Wait complete.");
        }
    }

    private Process? FindProcessByName(string name) => Process.GetProcesses().FirstOrDefault(process =>
        process.ProcessName.Contains(name, StringComparison.InvariantCultureIgnoreCase));

    private Task RestartBinaryExecutableAsync(string executablePath)
        => Task.Run(() => Process.Start(executablePath), _cancellationToken);

    private Task StartDesktopFileAsync(string desktopFilePath)
        => Task.Run(() => Process.Start("gio", ["launch", desktopFilePath]), _cancellationToken);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task = RunAsync(_cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sts.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _sts.Dispose();
    }
}