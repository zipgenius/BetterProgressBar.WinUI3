namespace ZipGenius.BetterProgressBar;

/// <summary>
/// Defines the visual and functional state of the <see cref="BetterProgressBar"/>.
/// </summary>
public enum ProgressBarState
{
    /// <summary>Normal progress state displayed in the default accent color (blue).</summary>
    Normal,

    /// <summary>Warning / attention state displayed in orange.</summary>
    Warning,

    /// <summary>Error state displayed in red.</summary>
    Error,

    /// <summary>Disabled state displayed in a desaturated gray color.</summary>
    Disabled,

    /// <summary>Indeterminate state with an animated fill indicating progress of unknown duration.</summary>
    Indeterminate
}

/// <summary>
/// Defines where tick marks are rendered relative to the progress bar track.
/// </summary>
public enum TickPlacement
{
    /// <summary>No tick marks are displayed.</summary>
    None,

    /// <summary>Tick marks are displayed above the bar.</summary>
    Above,

    /// <summary>Tick marks are displayed below the bar.</summary>
    Below,

    /// <summary>Tick marks are displayed both above and below the bar.</summary>
    Both
}

/// <summary>
/// Predefined visual themes for <see cref="BetterProgressBar"/>.
/// Applying a theme sets <see cref="BetterProgressBar.BarHeight"/>,
/// <see cref="BetterProgressBar.FillThickness"/>, <see cref="BetterProgressBar.TrackBrush"/>,
/// <see cref="BetterProgressBar.FillBrush"/>, and corner radius to match
/// the look of the named Windows version.
/// Individual appearance properties can still be overridden after the theme is applied.
/// </summary>
public enum ProgressBarTheme
{
    /// <summary>
    /// No predefined theme. All appearance properties keep their explicitly set values.
    /// </summary>
    None,

    /// <summary>
    /// Windows 11 look: 4 px tall, rounded corners, accent blue fill, subtle gray track.
    /// This is the default theme.
    /// </summary>
    Windows11,

    /// <summary>
    /// Windows 10 look: 20 px tall, no rounding, green fill (#06B025), light gray track (#C8C8C8),
    /// with a sweeping shimmer animation.
    /// </summary>
    Windows10,

    /// <summary>
    /// Windows 7 look: 20 px tall, no rounding, green fill (#06B025), sunken gray track,
    /// with a glassy top-highlight, diagonal stripe overlay, and sweeping shimmer animation.
    /// </summary>
    Windows7
}
