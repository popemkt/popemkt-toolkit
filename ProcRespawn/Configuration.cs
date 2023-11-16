namespace ProcRespawn;
class ProcessConfig
{
    public string Name { get; set; }
    public ExecutableType Type { get; set; } // "executable" or "desktop"
    public string Path { get; set; } // Executable path or .desktop file path
}

enum ExecutableType
{
    Binary,
    Desktop
}

class AppConfig
{
    public List<ProcessConfig> Processes { get; set; }
    public int IntervalInMilliseconds { get; set; }
}