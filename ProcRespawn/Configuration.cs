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
    public int StartupBackoffInMilliseconds { get; set; } = 10_000;
}