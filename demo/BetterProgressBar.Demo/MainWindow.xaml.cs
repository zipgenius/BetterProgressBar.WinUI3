using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using WinRT.Interop;
using ZipGenius.BetterProgressBar;

namespace BetterProgressBar.Demo;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set window size via AppWindow (Width/Height are not valid on WinUI 3 Window)
        nint hwnd = WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32(800, 700));

        // Wire up taskbar sync
        BarInteractive.SetTaskbarOwnerWindow(hwnd);

        SliderInteractive.Value = 25;
        BarInteractive.Value    = 25;
    }

    // Uses WinUI RangeBaseValueChangedEventArgs from the Slider, not our custom one
    private void SliderInteractive_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        => BarInteractive.Value = e.NewValue;

    private void SetStateNormal_Click(object sender, RoutedEventArgs e)
        => BarInteractive.ProgressState = ProgressBarState.Normal;

    private void SetStateWarning_Click(object sender, RoutedEventArgs e)
        => BarInteractive.ProgressState = ProgressBarState.Warning;

    private void SetStateError_Click(object sender, RoutedEventArgs e)
        => BarInteractive.ProgressState = ProgressBarState.Error;

    private void SetStateDisabled_Click(object sender, RoutedEventArgs e)
        => BarInteractive.ProgressState = ProgressBarState.Disabled;

    private void SetStateIndeterminate_Click(object sender, RoutedEventArgs e)
        => BarInteractive.ProgressState = ProgressBarState.Indeterminate;
}
