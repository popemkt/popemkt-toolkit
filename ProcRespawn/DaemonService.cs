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
        configMonitor.OnChange(_ => { logger.LogInformation("Configuration changed. Reloading..."); });
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting loop...");
            var config = _configMonitor.CurrentValue;

            foreach (var processConfig in config.Processes)
            {
                var process = FindProcessByName(processConfig.Name);

                if (process is not null && process?.HasExited is not true)
                    continue;

                _logger.LogInformation($"Starting process: {processConfig.Name}");

                switch (processConfig.Type)
                {
                    case ExecutableType.Binary:
                        await RestartBinaryExecutableAsync(processConfig.Path);
                        break;
                    case ExecutableType.Desktop:
                        await StartDesktopFileAsync(processConfig.Path);
                        break;
                    default:
                        _logger.LogError($"Unknown process type: {processConfig.Type}");
                        break;
                }
            }

            _logger.LogInformation("Waiting for next interval...");
            await Task.Delay(_configMonitor.CurrentValue.IntervalInMilliseconds,
                cancellationToken); // Adjust the interval as needed
        }
    }

    private Process? FindProcessByName(string name) => Process.GetProcesses().FirstOrDefault(process =>
        process.ProcessName.Contains(name, StringComparison.InvariantCultureIgnoreCase));

    private Task RestartBinaryExecutableAsync(string executablePath)
        => Task.Run(() => Process.Start(executablePath), _cancellationToken);

    private Task StartDesktopFileAsync(string desktopFilePath)
        => Task.Run(() => Process.Start("gio", ["launch", desktopFilePath]), _cancellationToken);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return RunAsync(stoppingToken);
    }

    public override void Dispose()
    {
        _sts.Dispose();
        base.Dispose();
    }
}