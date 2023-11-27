using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcRespawn;

sealed class ProcRespawnDaemon : IHostedService, IDisposable
{
    private readonly ILogger<ProcRespawnDaemon> _logger;
    private readonly IOptionsMonitor<AppConfig> _configMonitor;
    private Task? _task;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<ProcessConfig, ProcessWrapper> _processes = new();
    private readonly ILogger<ProcessWrapper> _processLogger;
    private readonly CancellationTokenSource _sts;

    public ProcRespawnDaemon(IOptionsMonitor<AppConfig> configMonitor, ILoggerFactory loggerFactory)
    {
        _sts = new CancellationTokenSource();
        _logger = loggerFactory.CreateLogger<ProcRespawnDaemon>();
        _processLogger = loggerFactory.CreateLogger<ProcessWrapper>();
        _configMonitor = configMonitor;
        configMonitor.OnChange(OnConfigChange);
    }

    private async void OnConfigChange(AppConfig _)
    {
        //TODO: for some bizarre reason, this is called twice when the config changes from the 2nd time forward
        //https://stackoverflow.com/questions/75257352/double-call-of-onchange-callback-happening-first-detection-of-change-by-ioptionm
        //https://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
        //Suggestions might be that filtering + throttling could work.
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Configuration changed. Reloading...");
            await Parallel.ForEachAsync(_processes.Values,
                (wrapper, cancellationToken) => wrapper.KillWithoutRestartAsync(cancellationToken));

            _processes.Clear();

            _task = StartOrMonitorProcessesAsync(_sts.Token);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task StartOrMonitorProcessesAsync(CancellationToken cancellationToken)
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
                    wrapper.StartOrMonitorSilently(startUpBackoff, cancellationToken);
                    return Task.CompletedTask;
                }

                wrapper.ProcessExited += OnWrapperProcessExited;
                _processes[processConfig] = wrapper;
                wrapper.StartOrMonitorSilently(startUpBackoff, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return StartOrMonitorProcessesAsync(_sts.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _sts.CancelAsync();
    }

    public void Dispose()
    {
        _sts.Dispose();
    }
}