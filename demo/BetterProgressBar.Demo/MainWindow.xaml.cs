using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.IO.Compression;
using WinRT.Interop;
using ZipGenius.BetterProgressBar;

namespace BetterProgressBar.Demo;

public sealed partial class MainWindow : Window
{
    private CancellationTokenSource? _heavyOperationCancellation;

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

    private async void ShowIndeterminateDialog_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new IndeterminateProgressDialog
        {
            XamlRoot = ((FrameworkElement)Content).XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void RunHeavyOperation_Click(object sender, RoutedEventArgs e)
    {
        _heavyOperationCancellation = new CancellationTokenSource();
        RunHeavyOperationButton.IsEnabled = false;
        CancelHeavyOperationButton.IsEnabled = true;
        BarHeavyOperation.Visibility = Visibility.Visible;
        BarHeavyOperation.ProgressState = ProgressBarState.Indeterminate;
        HeavyOperationStatusText.Text = "Creating and extracting a 128 MiB ZIP archive...";

        try
        {
            // Yield once after changing the visual state so the UI can present the bar before work starts.
            await Task.Delay(100);

            await Task.Factory.StartNew(
                () => CreateAndExtractLargeZip(_heavyOperationCancellation.Token),
                _heavyOperationCancellation.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            HeavyOperationStatusText.Text = "Completed. The animation stayed on the UI thread while extraction ran in the background.";
        }
        catch (OperationCanceledException)
        {
            HeavyOperationStatusText.Text = "Cancelled.";
        }
        catch (Exception ex)
        {
            HeavyOperationStatusText.Text = $"Test failed: {ex.Message}";
        }
        finally
        {
            BarHeavyOperation.Visibility = Visibility.Collapsed;
            CancelHeavyOperationButton.IsEnabled = false;
            RunHeavyOperationButton.IsEnabled = true;
            _heavyOperationCancellation?.Dispose();
            _heavyOperationCancellation = null;
        }
    }

    private void CancelHeavyOperation_Click(object sender, RoutedEventArgs e)
        => _heavyOperationCancellation?.Cancel();

    private static void CreateAndExtractLargeZip(CancellationToken cancellationToken)
    {
        const int payloadSize = 128 * 1024 * 1024;
        const int bufferSize = 1024 * 1024;
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"BetterProgressBarDemo-{Guid.NewGuid():N}");
        string zipPath = Path.Combine(workingDirectory, "large-file.zip");
        string extractionDirectory = Path.Combine(workingDirectory, "extracted");

        try
        {
            Directory.CreateDirectory(workingDirectory);
            byte[] buffer = new byte[bufferSize];
            var random = new Random(42);

            using (var zipFile = File.Create(zipPath))
            using (var archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
            using (var entryStream = archive.CreateEntry("large-file.bin", CompressionLevel.Fastest).Open())
            {
                for (int bytesWritten = 0; bytesWritten < payloadSize; bytesWritten += buffer.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    random.NextBytes(buffer);
                    entryStream.Write(buffer, 0, buffer.Length);
                }
            }

            Directory.CreateDirectory(extractionDirectory);
            using var readArchive = ZipFile.OpenRead(zipPath);
            using Stream input = readArchive.Entries.Single().Open();
            using Stream output = File.Create(Path.Combine(extractionDirectory, "large-file.bin"));
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                output.Write(buffer, 0, bytesRead);
            }
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
                Directory.Delete(workingDirectory, recursive: true);
        }
    }
}
