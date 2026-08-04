namespace SamWinOptimize;

internal static class Program
{
    private const string MutexName = @"Local\SamWinOptimize.SingleInstance";

    [STAThread]
    private static void Main()
    {
        using var singleInstance = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "SamWinOptimize 已在运行。请切换到现有窗口。",
                "SamWinOptimize",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetDefaultFont(UI.Theme.Font(9));
        Application.Run(new MainForm());
    }
}
