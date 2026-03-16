using System.Runtime.InteropServices;

namespace ZipGenius.BetterProgressBar;

/// <summary>
/// Taskbar button progress state flags used by <c>ITaskbarList3.SetProgressState</c>.
/// </summary>
internal enum TaskbarProgressState : int
{
    NoProgress    = 0x0,
    Indeterminate = 0x1,
    Normal        = 0x2,
    Error         = 0x4,
    Paused        = 0x8
}

/// <summary>
/// COM interface for ITaskbarList3 — enables Windows taskbar button progress.
/// </summary>
[ComImport]
[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ITaskbarList3
{
    // ITaskbarList
    [PreserveSig] void HrInit();
    [PreserveSig] void AddTab(nint hwnd);
    [PreserveSig] void DeleteTab(nint hwnd);
    [PreserveSig] void ActivateTab(nint hwnd);
    [PreserveSig] void SetActiveAlt(nint hwnd);

    // ITaskbarList2
    [PreserveSig] void MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

    // ITaskbarList3
    [PreserveSig] void SetProgressValue(nint hwnd, ulong ullCompleted, ulong ullTotal);
    [PreserveSig] void SetProgressState(nint hwnd, TaskbarProgressState tbpFlags);
}

[ComImport]
[Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
[ClassInterface(ClassInterfaceType.None)]
internal class TaskbarInstance { }

/// <summary>
/// Singleton helper that wraps <c>ITaskbarList3</c> to update the Windows taskbar
/// button progress indicator. All operations are best-effort and silently swallow
/// COM exceptions so that failures never crash the host application.
/// </summary>
internal static class TaskbarProgressHelper
{
    private static ITaskbarList3? _taskbar;
    private static bool _initialized;

    private static ITaskbarList3? GetTaskbar()
    {
        if (_initialized) return _taskbar;
        _initialized = true;
        try
        {
            _taskbar = (ITaskbarList3)new TaskbarInstance();
            _taskbar.HrInit();
        }
        catch
        {
            _taskbar = null;
        }
        return _taskbar;
    }

    /// <summary>
    /// Updates the progress value shown on the taskbar button.
    /// </summary>
    /// <param name="hwnd">Handle of the owner window.</param>
    /// <param name="value">Current value (must be ≤ <paramref name="maximum"/>).</param>
    /// <param name="maximum">Maximum value of the range.</param>
    public static void SetValue(nint hwnd, double value, double maximum)
    {
        var tb = GetTaskbar();
        if (tb is null || hwnd == 0) return;
        try
        {
            ulong completed = (ulong)Math.Max(0, Math.Min(value, maximum));
            ulong total     = (ulong)Math.Max(1, maximum);
            tb.SetProgressValue(hwnd, completed, total);
        }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Maps a <see cref="ProgressBarState"/> to the corresponding
    /// <see cref="TaskbarProgressState"/> and applies it to the taskbar button.
    /// </summary>
    /// <param name="hwnd">Handle of the owner window.</param>
    /// <param name="state">Current progress state.</param>
    public static void SetState(nint hwnd, ProgressBarState state)
    {
        var tb = GetTaskbar();
        if (tb is null || hwnd == 0) return;
        try
        {
            var tbState = state switch
            {
                ProgressBarState.Indeterminate => TaskbarProgressState.Indeterminate,
                ProgressBarState.Error         => TaskbarProgressState.Error,
                ProgressBarState.Warning       => TaskbarProgressState.Paused,
                ProgressBarState.Disabled      => TaskbarProgressState.NoProgress,
                _                              => TaskbarProgressState.Normal
            };
            tb.SetProgressState(hwnd, tbState);
        }
        catch { /* best-effort */ }
    }

    /// <summary>
    /// Removes the progress overlay from the taskbar button (sets state to NoProgress).
    /// </summary>
    /// <param name="hwnd">Handle of the owner window.</param>
    public static void Clear(nint hwnd)
    {
        var tb = GetTaskbar();
        if (tb is null || hwnd == 0) return;
        try { tb.SetProgressState(hwnd, TaskbarProgressState.NoProgress); }
        catch { /* best-effort */ }
    }
}
