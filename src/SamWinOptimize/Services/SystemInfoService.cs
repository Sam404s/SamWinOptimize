using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using SamWinOptimize.Models;

namespace SamWinOptimize.Services;

public sealed class SystemInfoService
{
    public Task<SystemSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default) =>
        Task.Run(CreateSnapshot, cancellationToken);

    private static SystemSnapshot CreateSnapshot()
    {
        var (windowsName, displayVersion, build) = ReadWindowsVersion();
        var processor = ReadProcessorName();
        var memory = ReadMemorySummary();
        var disk = ReadSystemDiskSummary();
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

        return new SystemSnapshot(
            windowsName,
            $"{displayVersion} · Build {build}",
            RuntimeInformation.OSArchitecture.ToString(),
            Environment.MachineName,
            processor,
            memory,
            disk,
            FormatUptime(uptime),
            IsAdministrator(),
            Process.GetProcessesByName("MsMpEng").Length > 0);
    }

    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static (string Name, string DisplayVersion, string Build) ReadWindowsVersion()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var name = key?.GetValue("ProductName")?.ToString() ?? "Windows";
        var version = key?.GetValue("DisplayVersion")?.ToString()
            ?? key?.GetValue("ReleaseId")?.ToString()
            ?? "未知版本";
        var build = key?.GetValue("CurrentBuildNumber")?.ToString() ?? Environment.OSVersion.Version.Build.ToString();
        return (name.Replace("Windows 10", "Windows 11", StringComparison.Ordinal), version, build);
    }

    private static string ReadProcessorName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        var value = key?.GetValue("ProcessorNameString")?.ToString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? $"{Environment.ProcessorCount} 逻辑处理器" : value;
    }

    private static string ReadMemorySummary()
    {
        var status = new MemoryStatusEx();
        if (!GlobalMemoryStatusEx(ref status))
        {
            return "无法读取";
        }

        var used = status.TotalPhysical - status.AvailablePhysical;
        return $"{FormatBytes(used)} / {FormatBytes(status.TotalPhysical)}";
    }

    private static string ReadSystemDiskSummary()
    {
        var root = Path.GetPathRoot(Environment.SystemDirectory);
        if (root is null)
        {
            return "无法读取";
        }

        var drive = new DriveInfo(root);
        var used = drive.TotalSize - drive.AvailableFreeSpace;
        return $"{FormatBytes((ulong)used)} / {FormatBytes((ulong)drive.TotalSize)}";
    }

    private static string FormatBytes(ulong bytes)
    {
        const double gib = 1024d * 1024d * 1024d;
        return $"{bytes / gib:0.0} GB";
    }

    private static string FormatUptime(TimeSpan value)
    {
        if (value.TotalDays >= 1)
        {
            return $"{(int)value.TotalDays} 天 {value.Hours} 小时";
        }

        return $"{value.Hours} 小时 {value.Minutes} 分钟";
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx status);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;

        public MemoryStatusEx()
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
            MemoryLoad = 0;
            TotalPhysical = 0;
            AvailablePhysical = 0;
            TotalPageFile = 0;
            AvailablePageFile = 0;
            TotalVirtual = 0;
            AvailableVirtual = 0;
            AvailableExtendedVirtual = 0;
        }
    }
}
