using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace ProcRespawn;

public class ProcessWrapper
{
    private readonly ProcessConfig _processConfig;
    private readonly ILogger<ProcessWrapper> _logger;
    private Process? _process;
    public event AsyncEventHandler<ProcessConfig> ProcessExited;
    private Task _task;
    private Task _restartTask;

    public ProcessWrapper(ILogger<ProcessWrapper> logger, ProcessConfig processConfig, Process? process = null)
    {
        _logger = logger;
        _process = process;
        _processConfig = processConfig;
    }

    public void StartOrMonitorSilently(TimeSpan delay, CancellationToken cancellationToken)
    {
        _task = StartOrMonitorAsync(cancellationToken, delay);
    }

    public async ValueTask KillWithoutRestartAsync(CancellationToken cancellationToken)
    {
        if (_process is null) return;
        ProcessExited = default!;
        _process.Exited -= OnProcessOnExited;
        _process!.Kill(true);
        //TODO: if this line is present, _process is null when OnChange fired twice
        // await _process.WaitForExitAsync();
        _process.Dispose();
        _process = null;
    }

    private async Task StartOrMonitorAsync(CancellationToken cancellationToken, TimeSpan delay)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(delay, cancellationToken);
        try
        {
            _process = ProcessUtils.FindProcessByName(_processConfig.Name);
            if (_process is null || _process?.HasExited is true)
            {
                switch (_processConfig.Type)
                {
                    case ExecutableType.Binary:
                        //TODO: this is untested
                        _process = Process.Start(_processConfig.Path);
                        break;
                    case ExecutableType.Desktop:
#if LINUX
                        var gioProcess = Process.Start("gio", ["launch", _processConfig.Path]);
                        await gioProcess.WaitForExitAsync(cancellationToken);
#else
                        Process.Start(_processConfig.Path);
#endif
                        // Wait for the process to kick up
                        while (_process is null && !cancellationToken.IsCancellationRequested)
                        {
                            _process = ProcessUtils.FindProcessByName(_processConfig.Name);
                            await Task.Delay(1000, cancellationToken);
                        }

                        break;
                    default:
                        _logger.LogError("Unknown process type: {ProcessConfigType}", _processConfig.Type);
                        break;
                }
            }

            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessOnExited;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error trying to start process");
            throw;
        }
    }

    private void OnProcessOnExited(object? o, EventArgs eventArgs)
    {
        _restartTask = ProcessExited.InvokeAsync(this, _processConfig);
    }
}