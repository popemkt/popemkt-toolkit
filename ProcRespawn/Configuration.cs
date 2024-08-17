namespace ProcRespawn;

public class ProcessConfig
{
    public string Name { get; set; }
    public ExecutableType Type { get; set; } = ExecutableType.Desktop; // "executable" or "desktop"
    public string Path { get; set; } // Executable path or .desktop file path
}

public enum ExecutableType
{
    Binary,
    Desktop
}

class AppConfig
{
    public List<ProcessConfig> Processes { get; set; } = new();
    /// <summary>
    /// For waiting a while before starting firing off processes when ProcRespawn is started at boot, usually because OS's startup programs take sometime to load
    /// Also used for delay between restarting processes
    /// </summary>
    public int StartupBackoffInMilliseconds { get; set; } = 10_000;
}