using System.Diagnostics;

namespace ProcRespawn;

static class ProcessUtils
{
    public static Process? FindProcessByName(string name) => Process.GetProcesses().FirstOrDefault(process =>
        process.ProcessName.Contains(name, StringComparison.InvariantCultureIgnoreCase));
}