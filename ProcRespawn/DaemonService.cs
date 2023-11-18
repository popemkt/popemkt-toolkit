using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcRespawn;

sealed class DaemonService : IHostedService, IDisposable
{
    private readonly ILogger<DaemonService> _logger;
    private readonly IOptionsMonitor<AppConfig> _configMonitor;
    private Task? _task;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<ProcessConfig, ProcessWrapper> _processes = new();
    private readonly ILogger<ProcessWrapper> _processLogger;
    private readonly CancellationTokenSource _sts;

    public DaemonService(IOptionsMonitor<AppConfig> configMonitor, ILoggerFactory loggerFactory)
    {
        _sts = new CancellationTokenSource();
        _logger = loggerFactory.CreateLogger<DaemonService>();
        _processLogger = loggerFactory.CreateLogger<ProcessWrapper>();
        _logger.LogInformation("Start DaemonService");
        _configMonitor = configMonitor;
        configMonitor.OnChange(OnConfigChange);
    }

    private async void OnConfigChange(AppConfig _)
    {
        //TODO: for some bizarre reason, this is called twice when the config changes from the 2nd time forward
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Configuration changed. Reloading...");
            await Parallel.ForEachAsync(_processes.Values,
                (wrapper, cancellationToken) => wrapper.KillWithoutRestartAsync(cancellationToken));

            _processes.Clear();

            _task = RunAsync(_sts.Token);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var config = _configMonitor.CurrentValue;
        var startUpBackoff = TimeSpan.FromMilliseconds(config.StartupBackoffInMilliseconds);

        foreach (var processConfig in config.Processes)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var wrapper = new ProcessWrapper(_processLogger, processConfig);

                Task OnWrapperProcessExited(object? __, ProcessConfig _)
                {
                    wrapper.StartSilent(cancellationToken, startUpBackoff);
                    return Task.CompletedTask;
                }

                wrapper.ProcessExited += OnWrapperProcessExited;
                _processes[processConfig] = wrapper;
                wrapper.StartSilent(cancellationToken, startUpBackoff);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _task = RunAsync(_sts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _sts.CancelAsync();
    }

    public void Dispose()
    {
        _sts.Dispose();
    }
}