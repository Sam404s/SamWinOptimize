using System.Diagnostics;

namespace SamWinOptimize.Services;

public static class SettingsLauncher
{
    public static void Open(string target)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
    }
}
