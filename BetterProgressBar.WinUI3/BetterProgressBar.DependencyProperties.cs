using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace ZipGenius.BetterProgressBar;

public sealed partial class BetterProgressBar
{
    // ────────────────────────────────────────────────────────────────────────
    // Minimum  (mirrors ProgressBar.Minimum)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(0.0, OnMinimumChanged));

    /// <summary>Gets or sets the minimum value of the range. Default: 0.</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (BetterProgressBar)d;
        if (ctrl._innerBar is not null) ctrl._innerBar.Minimum = (double)e.NewValue;
        ctrl.Value = CoerceValue(ctrl.Value, ctrl.Minimum, ctrl.Maximum);
        ctrl.RebuildTicks();
        ctrl.UpdatePercentageText();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Maximum  (mirrors ProgressBar.Maximum)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(100.0, OnMaximumChanged));

    /// <summary>Gets or sets the maximum value of the range. Default: 100.</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (BetterProgressBar)d;
        if (ctrl._innerBar is not null) ctrl._innerBar.Maximum = (double)e.NewValue;
        ctrl.Value = CoerceValue(ctrl.Value, ctrl.Minimum, ctrl.Maximum);
        ctrl.RebuildTicks();
        ctrl.UpdatePercentageText();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Value  (mirrors ProgressBar.Value)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(0.0, OnValueChanged));

    /// <summary>Gets or sets the current progress value. Clamped to [Minimum, Maximum]. Default: 0.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, CoerceValue(value, Minimum, Maximum));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl   = (BetterProgressBar)d;
        double newVal = (double)e.NewValue;
        double oldVal = (double)e.OldValue;
        ctrl.UpdateFillRect();
        ctrl.UpdatePercentageText();
        ctrl.SyncTaskbarState();
        ctrl.ValueChanged?.Invoke(ctrl, new BetterProgressBarValueChangedEventArgs(oldVal, newVal));
    }

    // ────────────────────────────────────────────────────────────────────────
    // IsIndeterminate  (mirrors ProgressBar.IsIndeterminate)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(BetterProgressBar),
            new PropertyMetadata(false, OnIsIndeterminateChanged));

    /// <summary>
    /// Gets or sets whether the bar shows indeterminate animation.
    /// Setting this to <c>true</c> automatically sets <see cref="ProgressState"/> to
    /// <see cref="ProgressBarState.Indeterminate"/> and vice versa.
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl        = (BetterProgressBar)d;
        bool indeterminate = (bool)e.NewValue;
        if (indeterminate && ctrl.ProgressState != ProgressBarState.Indeterminate)
            ctrl.ProgressState = ProgressBarState.Indeterminate;
        else if (!indeterminate && ctrl.ProgressState == ProgressBarState.Indeterminate)
            ctrl.ProgressState = ProgressBarState.Normal;
    }

    // ────────────────────────────────────────────────────────────────────────
    // ProgressState
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty ProgressStateProperty =
        DependencyProperty.Register(nameof(ProgressState), typeof(ProgressBarState), typeof(BetterProgressBar),
            new PropertyMetadata(ProgressBarState.Normal, OnProgressStateChanged));

    /// <summary>Gets or sets the visual and functional state of the progress bar. Default: Normal.</summary>
    public ProgressBarState ProgressState
    {
        get => (ProgressBarState)GetValue(ProgressStateProperty);
        set => SetValue(ProgressStateProperty, value);
    }

    private static void OnProgressStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl    = (BetterProgressBar)d;
        bool isIndet = (ProgressBarState)e.NewValue == ProgressBarState.Indeterminate;
        if ((bool)ctrl.GetValue(IsIndeterminateProperty) != isIndet)
            ctrl.SetValue(IsIndeterminateProperty, isIndet);
        ctrl.UpdateVisualState();
    }

    // ────────────────────────────────────────────────────────────────────────
    // SyncTaskbar
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty SyncTaskbarProperty =
        DependencyProperty.Register(nameof(SyncTaskbar), typeof(bool), typeof(BetterProgressBar),
            new PropertyMetadata(false, OnSyncTaskbarChanged));

    /// <summary>
    /// Gets or sets whether this bar drives the taskbar button progress indicator.
    /// Requires a valid HWND provided via <see cref="SetTaskbarOwnerWindow"/>.
    /// Default: false.
    /// </summary>
    public bool SyncTaskbar
    {
        get => (bool)GetValue(SyncTaskbarProperty);
        set => SetValue(SyncTaskbarProperty, value);
    }

    private static void OnSyncTaskbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (BetterProgressBar)d;
        if ((bool)e.NewValue)
            ctrl.SyncTaskbarState();
        else if (ctrl._hwnd != 0)
            TaskbarProgressHelper.Clear(ctrl._hwnd);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Ticks
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty TicksProperty =
        DependencyProperty.Register(nameof(Ticks), typeof(TickPlacement), typeof(BetterProgressBar),
            new PropertyMetadata(TickPlacement.None, OnTicksChanged));

    /// <summary>Gets or sets where tick marks are rendered relative to the bar. Default: None.</summary>
    public TickPlacement Ticks
    {
        get => (TickPlacement)GetValue(TicksProperty);
        set => SetValue(TicksProperty, value);
    }

    private static void OnTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).RebuildTicks();

    // ────────────────────────────────────────────────────────────────────────
    // TickFrequency  (number of equal divisions, 1–10)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(10.0, OnTickFrequencyChanged));

    /// <summary>
    /// Gets or sets the number of equal tick divisions across the range (1–10).
    /// For example, <c>10</c> produces a tick every 10 % of the range.
    /// Values outside [1, 10] are clamped. Default: 10.
    /// </summary>
    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, Math.Max(1.0, Math.Min(10.0, value)));
    }

    private static void OnTickFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).RebuildTicks();

    // ────────────────────────────────────────────────────────────────────────
    // ShowPercentage
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty ShowPercentageProperty =
        DependencyProperty.Register(nameof(ShowPercentage), typeof(bool), typeof(BetterProgressBar),
            new PropertyMetadata(false, OnShowPercentageChanged));

    /// <summary>
    /// Gets or sets whether the percentage label is displayed to the right of the bar,
    /// vertically centered. The text color follows the system foreground. Default: false.
    /// </summary>
    public bool ShowPercentage
    {
        get => (bool)GetValue(ShowPercentageProperty);
        set => SetValue(ShowPercentageProperty, value);
    }

    private static void OnShowPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).UpdatePercentageText();

    // ────────────────────────────────────────────────────────────────────────
    // BarHeight
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(nameof(BarHeight), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(8.0, OnBarHeightChanged));

    /// <summary>Gets or sets the height of the progress bar track in pixels. Default: 8.</summary>
    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    private static void OnBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).ApplyBarHeight();

    // ────────────────────────────────────────────────────────────────────────
    // FillThickness
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty FillThicknessProperty =
        DependencyProperty.Register(nameof(FillThickness), typeof(double), typeof(BetterProgressBar),
            new PropertyMetadata(0.0, OnFillThicknessChanged));

    /// <summary>
    /// Gets or sets the height of the fill stripe inside the track, in pixels.
    /// When <c>0</c> (default) the fill occupies the full <see cref="BarHeight"/>.
    /// Values greater than <see cref="BarHeight"/> are clamped automatically.
    /// </summary>
    public double FillThickness
    {
        get => (double)GetValue(FillThicknessProperty);
        set => SetValue(FillThicknessProperty, Math.Max(0, value));
    }

    private static void OnFillThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).UpdateFillRect();

    // ────────────────────────────────────────────────────────────────────────
    // Brush properties
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty FillBrushProperty =
        DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnColorPropertyChanged));

    /// <summary>Gets or sets the fill brush used in the Normal state. <c>null</c> uses the default accent blue.</summary>
    public Brush? FillBrush
    {
        get => (Brush?)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnColorPropertyChanged));

    /// <summary>Gets or sets the background brush for the track. <c>null</c> uses the default light gray.</summary>
    public Brush? TrackBrush
    {
        get => (Brush?)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public static readonly DependencyProperty WarningBrushProperty =
        DependencyProperty.Register(nameof(WarningBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnColorPropertyChanged));

    /// <summary>Gets or sets the fill brush used in the Warning state. <c>null</c> uses the default orange.</summary>
    public Brush? WarningBrush
    {
        get => (Brush?)GetValue(WarningBrushProperty);
        set => SetValue(WarningBrushProperty, value);
    }

    public static readonly DependencyProperty ErrorBrushProperty =
        DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnColorPropertyChanged));

    /// <summary>Gets or sets the fill brush used in the Error state. <c>null</c> uses the default red.</summary>
    public Brush? ErrorBrush
    {
        get => (Brush?)GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    public static readonly DependencyProperty DisabledBrushProperty =
        DependencyProperty.Register(nameof(DisabledBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnColorPropertyChanged));

    /// <summary>Gets or sets the fill brush used in the Disabled state. <c>null</c> uses the default desaturated gray.</summary>
    public Brush? DisabledBrush
    {
        get => (Brush?)GetValue(DisabledBrushProperty);
        set => SetValue(DisabledBrushProperty, value);
    }

    public static readonly DependencyProperty TickBrushProperty =
        DependencyProperty.Register(nameof(TickBrush), typeof(Brush), typeof(BetterProgressBar),
            new PropertyMetadata(null, OnTickBrushChanged));

    /// <summary>Gets or sets the brush used to draw tick marks. <c>null</c> uses the default gray.</summary>
    public Brush? TickBrush
    {
        get => (Brush?)GetValue(TickBrushProperty);
        set => SetValue(TickBrushProperty, value);
    }

    private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).ApplyColors();

    private static void OnTickBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).RebuildTicks();


    // ────────────────────────────────────────────────────────────────────────
    // Theme  (predefined visual preset)
    // ────────────────────────────────────────────────────────────────────────

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(ProgressBarTheme), typeof(BetterProgressBar),
            new PropertyMetadata(ProgressBarTheme.Windows11, OnThemeChanged));

    /// <summary>
    /// Gets or sets a predefined visual theme.
    /// <para>
    /// Setting this property applies a matching combination of <see cref="BarHeight"/>,
    /// <see cref="FillThickness"/>, <see cref="TrackBrush"/>, <see cref="FillBrush"/>,
    /// and corner radius. Use <see cref="ProgressBarTheme.None"/> to opt out of
    /// any preset and rely entirely on individual property values.
    /// </para>
    /// Default: <see cref="ProgressBarTheme.Windows11"/>.
    /// </summary>
    public ProgressBarTheme Theme
    {
        get => (ProgressBarTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((BetterProgressBar)d).ApplyTheme();

    // ────────────────────────────────────────────────────────────────────────
    // Events
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>Raised when the <see cref="Value"/> property changes.</summary>
    public event EventHandler<BetterProgressBarValueChangedEventArgs>? ValueChanged;
}

/// <summary>Provides data for the <see cref="BetterProgressBar.ValueChanged"/> event.</summary>
public sealed class BetterProgressBarValueChangedEventArgs : EventArgs
{
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The value after the change.</param>
    public BetterProgressBarValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>Gets the value before the change.</summary>
    public double OldValue { get; }

    /// <summary>Gets the value after the change.</summary>
    public double NewValue { get; }
}
