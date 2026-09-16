using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;

namespace BetterProgressBar.Demo;

public partial class App : Application
{
    private const string AppUserModelId = "ZipGenius.BetterProgressBar.WinUI3.Demo";
    private MainWindow? _window;

    public App()
    {
        // The Demo can be launched by an IDE or terminal that has its own AUMID.
        // Use a distinct taskbar group before creating any windows, otherwise a
        // normal progress state from that group masks TBPF_INDETERMINATE.
        _ = SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
        InitializeComponent();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
