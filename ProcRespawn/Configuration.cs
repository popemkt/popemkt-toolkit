namespace ProcRespawn;

class ProcessConfig
{
    public string Name { get; set; }
    public ExecutableType Type { get; set; } = ExecutableType.Desktop; // "executable" or "desktop"
    public string Path { get; set; } // Executable path or .desktop file path
}

enum ExecutableType
{
    Binary,
    Desktop
}

class AppConfig
{
    public List<ProcessConfig> Processes { get; set; } = new();
    public int IntervalInMilliseconds { get; set; } = 10_000;
}