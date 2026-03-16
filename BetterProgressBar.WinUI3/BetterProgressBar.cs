using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace ZipGenius.BetterProgressBar;

/// <summary>
/// An enhanced WinUI 3 ProgressBar that mirrors the standard ProgressBar API and adds:
/// taskbar progress integration, tick marks, named states (Normal / Warning / Error / Disabled / Indeterminate),
/// an optional percentage label displayed to the right of the bar, a configurable fill thickness,
/// predefined themes (Windows11, Windows10, Windows7) and full color / height customization.
/// </summary>
[TemplatePart(Name = PartRootGrid,         Type = typeof(Grid))]
[TemplatePart(Name = PartTrackGrid,        Type = typeof(Grid))]
[TemplatePart(Name = PartTrackBorder,      Type = typeof(Border))]
[TemplatePart(Name = PartWin7Border,       Type = typeof(Border))]
[TemplatePart(Name = PartWin7Vignette,     Type = typeof(Rectangle))]
[TemplatePart(Name = PartFillContainer,    Type = typeof(Grid))]
[TemplatePart(Name = PartFillRect,         Type = typeof(Rectangle))]
[TemplatePart(Name = PartShimmerRect,      Type = typeof(Rectangle))]
[TemplatePart(Name = PartGlassHighlight,   Type = typeof(Rectangle))]
[TemplatePart(Name = PartStripeCanvas,     Type = typeof(Canvas))]
[TemplatePart(Name = PartInnerProgressBar, Type = typeof(ProgressBar))]
[TemplatePart(Name = PartTicksAbove,       Type = typeof(Canvas))]
[TemplatePart(Name = PartTicksBelow,       Type = typeof(Canvas))]
[TemplatePart(Name = PartPercentageText,   Type = typeof(TextBlock))]
[TemplateVisualState(Name = StateNormal,        GroupName = GroupProgressState)]
[TemplateVisualState(Name = StateWarning,       GroupName = GroupProgressState)]
[TemplateVisualState(Name = StateError,         GroupName = GroupProgressState)]
[TemplateVisualState(Name = StateDisabled,      GroupName = GroupProgressState)]
[TemplateVisualState(Name = StateIndeterminate, GroupName = GroupProgressState)]
public sealed partial class BetterProgressBar : Control
{
    // ── Template part names ──────────────────────────────────────────────────
    private const string PartRootGrid         = "PART_RootGrid";
    private const string PartTrackGrid        = "PART_TrackGrid";
    private const string PartTrackBorder      = "PART_TrackBorder";
    private const string PartWin7Border       = "PART_Win7Border";
    private const string PartWin7Vignette     = "PART_Win7Vignette";
    private const string PartFillContainer    = "PART_FillContainer";
    private const string PartFillRect         = "PART_FillRect";
    private const string PartShimmerRect      = "PART_ShimmerRect";
    private const string PartGlassHighlight   = "PART_GlassHighlight";
    private const string PartStripeCanvas     = "PART_StripeCanvas";
    private const string PartInnerProgressBar = "PART_InnerProgressBar";
    private const string PartTicksAbove       = "PART_TicksAbove";
    private const string PartTicksBelow       = "PART_TicksBelow";
    private const string PartPercentageText   = "PART_PercentageText";

    // ── Visual state names ───────────────────────────────────────────────────
    private const string GroupProgressState   = "ProgressState";
    private const string StateNormal          = "Normal";
    private const string StateWarning         = "Warning";
    private const string StateError           = "Error";
    private const string StateDisabled        = "Disabled";
    private const string StateIndeterminate   = "Indeterminate";

    // ── Template parts ───────────────────────────────────────────────────────
    private Grid?        _rootGrid;
    private Grid?        _trackGrid;
    private Border?      _trackBorder;
    private Border?      _win7Border;
    private Rectangle?   _win7Vignette;
    private Brush?       _cachedTrackBrush;
    private Grid?        _fillContainer;
    private Rectangle?   _fillRect;
    private Rectangle?   _shimmerRect;
    private Rectangle?   _glassHighlight;
    private Canvas?      _stripeCanvas;
    private ProgressBar? _innerBar;
    private Canvas?      _ticksAbove;
    private Canvas?      _ticksBelow;
    private TextBlock?   _percentageText;

    // ── Shimmer state ────────────────────────────────────────────────────────
    private Storyboard?  _shimmerStoryboard;
    private bool         _shimmerScheduled;
    private Microsoft.UI.Dispatching.DispatcherQueueTimer? _shimmerTimer;
    private double       _shimmerX;
    private double       _shimmerHoldFrames;
    private ProgressBarTheme _lastDecoratedTheme = (ProgressBarTheme)(-1);

    // ── Taskbar window handle ────────────────────────────────────────────────
    private nint _hwnd;

    // ────────────────────────────────────────────────────────────────────────
    // Constructor
    // ────────────────────────────────────────────────────────────────────────

    public BetterProgressBar()
    {
        DefaultStyleKey = typeof(BetterProgressBar);
        SizeChanged += (_, _) => { RebuildTicks(); UpdateFillRect(); };
        Unloaded    += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopShimmer();
        if (SyncTaskbar && _hwnd != 0)
            TaskbarProgressHelper.Clear(_hwnd);
    }

    // ────────────────────────────────────────────────────────────────────────
    // OnApplyTemplate
    // ────────────────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _rootGrid       = GetTemplateChild(PartRootGrid)         as Grid;
        _trackGrid      = GetTemplateChild(PartTrackGrid)        as Grid;
        _trackBorder    = GetTemplateChild(PartTrackBorder)      as Border;
        _win7Border     = GetTemplateChild(PartWin7Border)       as Border;
        _win7Vignette   = GetTemplateChild(PartWin7Vignette)     as Rectangle;
        _fillContainer  = GetTemplateChild(PartFillContainer)    as Grid;
        _fillRect       = GetTemplateChild(PartFillRect)         as Rectangle;
        _shimmerRect    = GetTemplateChild(PartShimmerRect)      as Rectangle;
        _glassHighlight = GetTemplateChild(PartGlassHighlight)   as Rectangle;
        _stripeCanvas   = GetTemplateChild(PartStripeCanvas)     as Canvas;
        _innerBar       = GetTemplateChild(PartInnerProgressBar) as ProgressBar;
        _ticksAbove     = GetTemplateChild(PartTicksAbove)       as Canvas;
        _ticksBelow     = GetTemplateChild(PartTicksBelow)       as Canvas;
        _percentageText = GetTemplateChild(PartPercentageText)   as TextBlock;

        if (_innerBar is not null)
        {
            _innerBar.Minimum         = Minimum;
            _innerBar.Maximum         = Maximum;
            _innerBar.IsIndeterminate = (ProgressState == ProgressBarState.Indeterminate);
        }

        ApplyTheme();
        ApplyBarHeight();
        ApplyColors();
        UpdateVisualState(useTransitions: false);
        UpdateFillRect();
        RebuildTicks();
        UpdatePercentageText();
        SyncTaskbarState();
        ScheduleDecoration();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Public API
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Provides the HWND of the application window whose taskbar button should
    /// reflect this ProgressBar's state and value.
    /// Call this once after the window has been created, e.g. in the MainWindow constructor.
    /// </summary>
    public void SetTaskbarOwnerWindow(nint hwnd)
    {
        _hwnd = hwnd;
        SyncTaskbarState();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Internal helpers
    // ────────────────────────────────────────────────────────────────────────

    private void UpdateVisualState(bool useTransitions = true)
    {
        string stateName = ProgressState switch
        {
            ProgressBarState.Warning       => StateWarning,
            ProgressBarState.Error         => StateError,
            ProgressBarState.Disabled      => StateDisabled,
            ProgressBarState.Indeterminate => StateIndeterminate,
            _                              => StateNormal
        };
        VisualStateManager.GoToState(this, stateName, useTransitions);

        if (_innerBar is not null)
            _innerBar.IsIndeterminate = (ProgressState == ProgressBarState.Indeterminate);

        ApplyColors();
        SyncTaskbarState();
        UpdatePercentageText();

        bool shimmerActive = _shimmerStoryboard is not null;
        bool shouldShimmer = IsShimmerTheme() && ProgressState != ProgressBarState.Indeterminate
                                              && ProgressState != ProgressBarState.Disabled;

        if (!shouldShimmer && shimmerActive)
            StopShimmer();
        else if (shouldShimmer && !shimmerActive)
            ScheduleShimmer();
    }

    private void ApplyBarHeight()
    {
        if (_trackBorder is not null) _trackBorder.Height = BarHeight;
        if (_innerBar is not null)
        {
            _innerBar.Height    = BarHeight;
            _innerBar.MinHeight = 0;
        }
        UpdateFillRect();
    }

    private void UpdateFillRect()
    {
        if (_fillRect is null || _fillContainer is null || _trackGrid is null) return;

        double trackWidth = _trackGrid.ActualWidth;
        if (trackWidth <= 0 && _trackBorder is not null)
            trackWidth = _trackBorder.ActualWidth;
        if (trackWidth <= 0) return;

        double range     = Maximum - Minimum;
        double ratio     = range > 0 ? (Value - Minimum) / range : 0;
        double fillWidth = ratio >= 1.0 ? trackWidth : Math.Max(0, ratio * trackWidth);
        double ft        = FillThickness > 0 ? Math.Min(FillThickness, BarHeight) : BarHeight;

        if (_fillContainer.Width  != fillWidth) _fillContainer.Width  = fillWidth;
        if (_fillContainer.Height != ft)        _fillContainer.Height = ft;
        if (_fillRect.Width       != fillWidth) _fillRect.Width       = fillWidth;
        if (_fillRect.Height      != ft)        _fillRect.Height      = ft;
    }

    private void ApplyColors()
    {
        if (_trackBorder is null || _fillRect is null) return;

        var newTrackBrush = TrackBrush ?? new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0));
        if (_cachedTrackBrush != newTrackBrush)
        {
            _cachedTrackBrush       = newTrackBrush;
            _trackBorder.Background = newTrackBrush;
        }

        Brush solidBrush = ProgressState switch
        {
            ProgressBarState.Warning  => WarningBrush  ?? new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x8C, 0x00)),
            ProgressBarState.Error    => ErrorBrush    ?? new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0x26, 0x26)),
            ProgressBarState.Disabled => DisabledBrush ?? new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0)),
            _                         => FillBrush     ?? new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4))
        };

        Brush fillBrush;
        if (Theme == ProgressBarTheme.Windows7)
        {
            var baseColor = solidBrush is SolidColorBrush scb ? scb.Color
                          : Color.FromArgb(0xFF, 0x0B, 0xD8, 0x2C);

            var gb = new LinearGradientBrush
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint   = new Windows.Foundation.Point(0, 1)
            };
            gb.GradientStops.Add(new GradientStop { Color = baseColor,                                                               Offset = 0.00 });
            gb.GradientStops.Add(new GradientStop { Color = baseColor,                                                               Offset = 0.40 });
            gb.GradientStops.Add(new GradientStop { Color = BlendOver(baseColor, Color.FromArgb(0x33, 0xCA, 0xCA, 0xCA)),            Offset = 0.40 });
            gb.GradientStops.Add(new GradientStop { Color = BlendOver(baseColor, Color.FromArgb(0x33, 0xD5, 0xD5, 0xD5)),            Offset = 0.65 });
            gb.GradientStops.Add(new GradientStop { Color = BlendOver(BlendOver(baseColor, Color.FromArgb(0x33, 0xD5, 0xD5, 0xD5)),
                                                                        Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF)),                      Offset = 1.00 });
            fillBrush = gb;
        }
        else
        {
            fillBrush = solidBrush;
        }

        _fillRect.Fill = fillBrush;
        if (_innerBar is not null) _innerBar.Foreground = solidBrush;
    }

    private void SyncTaskbarState()
    {
        if (!SyncTaskbar || _hwnd == 0) return;
        TaskbarProgressHelper.SetState(_hwnd, ProgressState);
        if (ProgressState != ProgressBarState.Indeterminate &&
            ProgressState != ProgressBarState.Disabled)
            TaskbarProgressHelper.SetValue(_hwnd, Value, Maximum);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Tick marks
    // ────────────────────────────────────────────────────────────────────────

    private void RebuildTicks()
    {
        bool showAbove = Ticks == TickPlacement.Above || Ticks == TickPlacement.Both;
        bool showBelow = Ticks == TickPlacement.Below || Ticks == TickPlacement.Both;

        if (_ticksAbove is not null)
        {
            _ticksAbove.Visibility = showAbove ? Visibility.Visible : Visibility.Collapsed;
            if (showAbove) DrawTicks(_ticksAbove, flipped: true);
        }
        if (_ticksBelow is not null)
        {
            _ticksBelow.Visibility = showBelow ? Visibility.Visible : Visibility.Collapsed;
            if (showBelow) DrawTicks(_ticksBelow, flipped: false);
        }
    }

    private void DrawTicks(Canvas canvas, bool flipped)
    {
        canvas.Children.Clear();

        double width = canvas.ActualWidth;
        if (width <= 0) return;

        double range = Maximum - Minimum;
        if (range <= 0) return;

        const double TickHeightEndpoint     = 14.0;
        const double TickHeightMid          =  9.0;
        const double TickHeightIntermediate =  5.0;
        double canvasH = canvas.Height;

        int    divisions = Math.Max(1, Math.Min(10, (int)Math.Round(TickFrequency)));
        double step      = range / divisions;

        var positions = new HashSet<double> { Minimum, Minimum + range * 0.5, Maximum };
        for (int i = 0; i <= divisions; i++)
            positions.Add(Math.Round(Minimum + i * step, 8));

        var tickBrush = TickBrush ?? new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));

        foreach (double pos in positions.OrderBy(x => x))
        {
            double ratio = (pos - Minimum) / range;
            double x     = ratio * width;

            bool isEndpoint = (pos <= Minimum + 1e-9 || pos >= Maximum - 1e-9);
            bool isMid      = Math.Abs(pos - (Minimum + range * 0.5)) < 1e-9;

            double tickH = isEndpoint ? TickHeightEndpoint
                         : isMid     ? TickHeightMid
                         :             TickHeightIntermediate;

            double strokeThickness = (isEndpoint || isMid) ? 2.0 : 1.5;
            double halfStroke = strokeThickness / 2.0;
            x = Math.Max(halfStroke, Math.Min(width - halfStroke, x));

            double y1 = flipped ? canvasH - tickH : 0;
            double y2 = flipped ? canvasH          : tickH;

            canvas.Children.Add(new Line
            {
                X1              = x,
                Y1              = y1,
                X2              = x,
                Y2              = y2,
                Stroke          = tickBrush,
                StrokeThickness = strokeThickness
            });
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Percentage label
    // ────────────────────────────────────────────────────────────────────────

    private void UpdatePercentageText()
    {
        if (_percentageText is null) return;
        _percentageText.Visibility = ShowPercentage ? Visibility.Visible : Visibility.Collapsed;
        if (!ShowPercentage) return;
        double range   = Maximum - Minimum;
        double percent = range > 0 ? (Value - Minimum) / range * 100.0 : 0;
        _percentageText.Text = $"{percent:F0}%";
    }

    // ────────────────────────────────────────────────────────────────────────
    // Theme presets
    // ────────────────────────────────────────────────────────────────────────

    internal void ApplyTheme()
    {
        switch (Theme)
        {
            case ProgressBarTheme.Windows11:
                SetValue(BarHeightProperty,     4.0);
                SetValue(FillThicknessProperty, 0.0);
                SetValue(FillBrushProperty,     (object?)null);
                SetValue(TrackBrushProperty,    (object?)null);
                SetCornerRadius(4.0);
                ApplyWin7OuterBorder(false);
                break;

            case ProgressBarTheme.Windows10:
                SetValue(BarHeightProperty,     20.0);
                SetValue(FillThicknessProperty, 20.0);
                SetValue(FillBrushProperty,     (Brush?)new SolidColorBrush(Color.FromArgb(0xFF, 0x06, 0xB0, 0x25)));
                SetValue(TrackBrushProperty,    (Brush?)new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0xC8, 0xC8)));
                SetCornerRadius(0.0);
                ApplyWin7OuterBorder(false);
                break;

            case ProgressBarTheme.Windows7:
                SetValue(BarHeightProperty,     20.0);
                SetValue(FillThicknessProperty, 20.0);
                SetValue(FillBrushProperty,     (Brush?)new SolidColorBrush(Color.FromArgb(0xFF, 0x06, 0xB0, 0x25)));
                SetValue(TrackBrushProperty,    (Brush?)MakeWin7TrackBrush());
                SetCornerRadius(3.0);
                ApplyWin7OuterBorder(true);
                break;

            case ProgressBarTheme.None:
            default:
                ApplyWin7OuterBorder(false);
                break;
        }

        _lastDecoratedTheme  = (ProgressBarTheme)(-1);
        _cachedTrackBrush    = null;

        ApplyBarHeight();
        ApplyColors();
        UpdateFillRect();
        ScheduleDecoration();
    }

    private void SetCornerRadius(double radius)
    {
        if (_trackBorder   is not null) _trackBorder.CornerRadius = new CornerRadius(radius);
        if (_fillRect      is not null) { _fillRect.RadiusX = radius; _fillRect.RadiusY = radius; }
        if (_fillContainer is not null) _fillContainer.CornerRadius = new CornerRadius(radius);
    }

    private void ApplyWin7OuterBorder(bool visible)
    {
        if (_win7Border is null) return;
        if (!visible)
        {
            _win7Border.BorderBrush     = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            _win7Border.BorderThickness = new Thickness(0);
            _win7Border.Padding         = new Thickness(0);
            _win7Border.Background      = null;
            return;
        }

        var borderBrush = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint   = new Windows.Foundation.Point(0, 1)
        };
        borderBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0x5A, 0x5A, 0x5A), Offset = 0.0 });
        borderBrush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0x8A, 0x8A, 0x8A), Offset = 1.0 });

        _win7Border.BorderBrush     = borderBrush;
        _win7Border.BorderThickness = new Thickness(1);
        _win7Border.CornerRadius    = new CornerRadius(3);
        _win7Border.Padding         = new Thickness(0);
        _win7Border.Background      = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Decoration scheduling (shimmer + Win7 glass)
    // ────────────────────────────────────────────────────────────────────────

    private bool IsShimmerTheme() =>
        Theme == ProgressBarTheme.Windows10 || Theme == ProgressBarTheme.Windows7;

    private void ScheduleDecoration()
    {
        if (_shimmerScheduled) return;
        _shimmerScheduled = true;
        _ = DispatcherQueue.TryEnqueue(
            Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
            RunDecoration);
    }

    private void ScheduleShimmer() => ScheduleDecoration();

    private void RunDecoration()
    {
        _shimmerScheduled = false;

        bool shouldShimmer = IsShimmerTheme()
                          && ProgressState != ProgressBarState.Indeterminate
                          && ProgressState != ProgressBarState.Disabled;

        if (!shouldShimmer)
            StopShimmer();
        else if (_shimmerStoryboard is null)
            StartShimmer();

        if (Theme == ProgressBarTheme.Windows7)
        {
            if (_lastDecoratedTheme != ProgressBarTheme.Windows7)
            {
                DrawGlassHighlight();
                DrawWin7Vignette();
                if (_stripeCanvas is not null)
                {
                    _stripeCanvas.Children.Clear();
                    _stripeCanvas.Visibility = Visibility.Collapsed;
                }
                _lastDecoratedTheme = ProgressBarTheme.Windows7;
            }
        }
        else
        {
            if (_glassHighlight is not null) _glassHighlight.Visibility  = Visibility.Collapsed;
            if (_stripeCanvas   is not null) { _stripeCanvas.Children.Clear(); _stripeCanvas.Visibility = Visibility.Collapsed; }
            if (_win7Vignette   is not null) _win7Vignette.Visibility    = Visibility.Collapsed;
            _lastDecoratedTheme = Theme;
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Shimmer animation
    // ────────────────────────────────────────────────────────────────────────

    private void StartShimmer()
    {
        if (_shimmerRect is null || _fillContainer is null) return;

        double fillWidth  = _fillContainer.ActualWidth;
        double fillHeight = _fillContainer.ActualHeight;
        if (fillWidth <= 0 || fillHeight <= 0) return;

        StopShimmer();

        double bandWidth = Math.Max(20, fillWidth * 0.40);

        _fillContainer.Clip = new RectangleGeometry
        {
            Rect = new Windows.Foundation.Rect(0, 0, fillWidth, fillHeight)
        };

        var gb = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint   = new Windows.Foundation.Point(1, 0)
        };
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0,   0xFF, 0xFF, 0xFF), Offset = 0.00 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(128, 0xFF, 0xFF, 0xFF), Offset = 0.20 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0,   0xFF, 0xFF, 0xFF), Offset = 0.40 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0,   0xFF, 0xFF, 0xFF), Offset = 1.00 });

        _shimmerRect.Fill                = gb;
        _shimmerRect.Width               = bandWidth;
        _shimmerRect.Height              = fillHeight;
        _shimmerRect.HorizontalAlignment = HorizontalAlignment.Left;
        _shimmerRect.VerticalAlignment   = VerticalAlignment.Top;
        _shimmerRect.Visibility          = Visibility.Visible;

        var translate = new TranslateTransform { X = -bandWidth };
        _shimmerRect.RenderTransform = translate;
        _shimmerX          = -bandWidth;
        _shimmerHoldFrames = 0;

        const double totalCycleMs = 3000.0;
        const double sweepFracMs  = 1800.0;
        const double fps          = 60.0;
        const double intervalMs   = 1000.0 / fps;

        _shimmerTimer             = DispatcherQueue.CreateTimer();
        _shimmerTimer.Interval    = TimeSpan.FromMilliseconds(intervalMs);
        _shimmerTimer.IsRepeating = true;
        _shimmerTimer.Tick += (_, _) =>
        {
            if (_shimmerRect is null || _fillContainer is null) return;

            double fw = _fillContainer.ActualWidth;
            if (fw <= 0) return;

            double bw            = Math.Max(20, fw * 0.40);
            double totalTravel   = fw + bw;
            double framesInSweep = sweepFracMs / intervalMs;
            double speed         = totalTravel / framesInSweep;

            if (_shimmerX < fw)
            {
                _shimmerX += speed;
            }
            else
            {
                _shimmerHoldFrames++;
                double holdFrames = (totalCycleMs - sweepFracMs) / intervalMs;
                if (_shimmerHoldFrames >= holdFrames)
                {
                    _shimmerX          = -bw;
                    _shimmerHoldFrames = 0;
                }
            }

            _shimmerRect.Width = bw;
            if (_shimmerRect.RenderTransform is TranslateTransform tt)
                tt.X = _shimmerX;

            if (_fillContainer.Clip is RectangleGeometry existingClip)
                existingClip.Rect = new Windows.Foundation.Rect(0, 0, fw, _fillContainer.ActualHeight);
            else
                _fillContainer.Clip = new RectangleGeometry
                    { Rect = new Windows.Foundation.Rect(0, 0, fw, _fillContainer.ActualHeight) };
        };

        _shimmerStoryboard = new Storyboard();
        _shimmerTimer.Start();
    }

    private void StopShimmer()
    {
        _shimmerTimer?.Stop();
        _shimmerTimer      = null;
        _shimmerStoryboard = null;
        if (_shimmerRect   is not null) _shimmerRect.Visibility = Visibility.Collapsed;
        if (_fillContainer is not null) _fillContainer.Clip     = null;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Windows 7 decoration
    // ────────────────────────────────────────────────────────────────────────

    private void DrawGlassHighlight()
    {
        if (_glassHighlight is null || _fillContainer is null) return;

        double h = _fillContainer.ActualHeight;
        if (h <= 0) return;

        double highlightH = Math.Max(2, h * 0.40);

        var brush = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint   = new Windows.Foundation.Point(0, 1)
        };
        brush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xAF, 0xF3, 0xF3, 0xF3), Offset = 0.00 });
        brush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xAF, 0xFC, 0xFC, 0xFC), Offset = 0.50 });
        brush.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xAF, 0xDB, 0xDB, 0xDB), Offset = 1.00 });

        _glassHighlight.Fill       = brush;
        _glassHighlight.Height     = highlightH;
        _glassHighlight.Visibility = Visibility.Visible;
    }

    private void DrawWin7Vignette()
    {
        if (_win7Vignette is null) return;

        var vb = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint   = new Windows.Foundation.Point(1, 0)
        };
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x1F, 0, 0, 0), Offset = 0.000 });
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x10, 0, 0, 0), Offset = 0.040 });
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x00, 0, 0, 0), Offset = 0.120 });
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x00, 0, 0, 0), Offset = 0.880 });
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x10, 0, 0, 0), Offset = 0.960 });
        vb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0x1F, 0, 0, 0), Offset = 1.000 });

        _win7Vignette.Fill       = vb;
        _win7Vignette.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Alpha-composites <paramref name="overlay"/> over <paramref name="baseC"/> (Porter-Duff src-over).
    /// </summary>
    private static Color BlendOver(Color baseC, Color overlay)
    {
        float a = overlay.A / 255f;
        byte R  = (byte)(overlay.R * a + baseC.R * (1 - a));
        byte G  = (byte)(overlay.G * a + baseC.G * (1 - a));
        byte B  = (byte)(overlay.B * a + baseC.B * (1 - a));
        return Color.FromArgb(0xFF, R, G, B);
    }

    /// <summary>
    /// Creates the sunken-channel gradient brush used as the track background
    /// in the Windows 7 theme.
    /// </summary>
    private static LinearGradientBrush MakeWin7TrackBrush()
    {
        var gb = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint   = new Windows.Foundation.Point(0, 1)
        };
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xEB, 0xEB, 0xEB), Offset = 0.00 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xF2, 0xF2, 0xF2), Offset = 0.20 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xDB, 0xDB, 0xDB), Offset = 0.40 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xCA, 0xCA, 0xCA), Offset = 0.40 });
        gb.GradientStops.Add(new GradientStop { Color = Color.FromArgb(0xFF, 0xD7, 0xD7, 0xD7), Offset = 1.00 });
        return gb;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Value coercion
    // ────────────────────────────────────────────────────────────────────────

    private static double CoerceValue(double value, double minimum, double maximum)
        => Math.Max(minimum, Math.Min(value, maximum));
}
