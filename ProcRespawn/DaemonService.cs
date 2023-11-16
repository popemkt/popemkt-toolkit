using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcRespawn;

sealed class DaemonService : BackgroundService, IDisposable
{
    private readonly ILogger<DaemonService> _logger;
    private readonly IOptionsMonitor<AppConfig> _configMonitor;
    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _sts;
    private Task? _task;

    public DaemonService(ILogger<DaemonService> logger, IOptionsMonitor<AppConfig> configMonitor)
    {
        logger.LogInformation("Start DaemonService");
        _sts = new CancellationTokenSource();
        _cancellationToken = _sts.Token;
        _logger = logger;
        _configMonitor = configMonitor;
        // Register a callback to handle configuration changes
        configMonitor.OnChange(_ =>
        {
            logger.LogInformation("Configuration changed. Reloading...");
        });
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting loop...");
            var config = _configMonitor.CurrentValue;

            foreach (var processConfig in config.Processes)
            {
                var process = FindProcessByName(processConfig.Name);

                if (process is not null)
                {
                    _logger.LogInformation($"Process found: {processConfig.Name}");
                }
                else if (process is null || process?.HasExited is true)
                {
                    _logger.LogInformation($"Restarting process: {processConfig.Name}");

                    switch (processConfig.Type)
                    {
                        case ExecutableType.Binary:
                            _logger.LogInformation("Binary");
                            await RestartBinaryExecutableAsync(processConfig.Path);
                            break;
                        case ExecutableType.Desktop:
                            _logger.LogInformation("Desktop");
                            await StartDesktopFileAsync(processConfig.Path);
                            break;
                        default:
                            Console.WriteLine($"Unknown process type: {processConfig.Type}");
                            break;
                    }
                }
            }

            _logger.LogInformation("Waiting for next interval...");
            await Task.Delay(_configMonitor.CurrentValue.IntervalInMilliseconds,
                cancellationToken); // Adjust the interval as needed
            _logger.LogInformation("Wait complete.");
        }
    }

    private Process? FindProcessByName(string name) => Process.GetProcesses().FirstOrDefault(process =>
        process.ProcessName.Contains(name, StringComparison.InvariantCultureIgnoreCase));

    private Task RestartBinaryExecutableAsync(string executablePath)
        => Task.Run(() => Process.Start(executablePath), _cancellationToken);

    private Task StartDesktopFileAsync(string desktopFilePath)
        => Task.Run(() =>
        {
            _logger.LogInformation(desktopFilePath);
            var process = Process.Start("gio", ["launch", desktopFilePath]);
            _logger.LogInformation(process.ProcessName + process.HasExited);
            process.WaitForExit();

            _logger.LogInformation("process exited" + process.ExitCode);
        }, _cancellationToken);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return RunAsync(stoppingToken);
    }

    public void Dispose()
    {
        _sts.Dispose();
    }
}