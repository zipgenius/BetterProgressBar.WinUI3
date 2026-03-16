# BetterProgressBar.WinUI3

[![NuGet](https://img.shields.io/nuget/v/BetterProgressBar.WinUI3.svg)](https://www.nuget.org/packages/BetterProgressBar.WinUI3/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![CI](https://github.com/zipgenius/BetterProgressBar.WinUI3/actions/workflows/ci.yml/badge.svg)](https://github.com/zipgenius/BetterProgressBar.WinUI3/actions)

An enhanced **WinUI 3** `ProgressBar` control published by [ZipGenius](https://www.zipgenius.it).
It mirrors the full API of the standard WinUI 3 `ProgressBar` and adds a rich set of opt-in features,
including predefined themes that replicate the classic Windows 7, Windows 10 and Windows 11 progress bar looks.

---

## Features

| Feature | Default | Description |
|---|---|---|
| **Predefined themes** | `Windows11` | Built-in Windows 11, Windows 10 and Windows 7 visual presets |
| **Taskbar progress** | off | Drives the Windows taskbar button indicator via `ITaskbarList3` |
| **Tick marks** | `None` | Renders ticks Above, Below, or Both sides of the bar |
| **Named states** | `Normal` | Normal · Warning · Error · Disabled · Indeterminate |
| **Percentage label** | off | Completion percentage displayed to the right of the bar |
| **Fill thickness** | `0` (= full) | Independent height of the fill stripe inside the track |
| **Custom colors** | system defaults | `FillBrush`, `TrackBrush`, `WarningBrush`, `ErrorBrush`, `DisabledBrush`, `TickBrush` |
| **Custom height** | `8` px | `BarHeight` property |

---

## Requirements

- Windows App SDK 1.8+
- .NET 8, 9, or 10
- Windows 10 1809 (build 17763) or later

---

## Installation

```
dotnet add package BetterProgressBar.WinUI3
```

Or search for **BetterProgressBar.WinUI3** in the NuGet Package Manager inside Visual Studio.

---

## Quick start

### 1. Merge the resource dictionary in App.xaml

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
            <ResourceDictionary Source="ms-appx:///BetterProgressBar.WinUI3/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2. Add the XML namespace

```xml
xmlns:ctrl="using:ZipGenius.BetterProgressBar"
```

### 3. Drop the control into your XAML

```xml
<!-- Default Windows 11 theme -->
<ctrl:BetterProgressBar Value="65" />

<!-- Explicit Windows 11 theme with ticks and percentage -->
<ctrl:BetterProgressBar Value="40"
                        Theme="Windows11"
                        BarHeight="14"
                        Ticks="Both"
                        TickFrequency="10"
                        ShowPercentage="True" />

<!-- Warning state -->
<ctrl:BetterProgressBar Value="80" ProgressState="Warning" />

<!-- Indeterminate -->
<ctrl:BetterProgressBar ProgressState="Indeterminate" />
```

### 4. Taskbar integration (code-behind)

```csharp
// In your MainWindow constructor, after the window is created:
using WinRT.Interop;

nint hwnd = WindowNative.GetWindowHandle(this);
MyBar.SetTaskbarOwnerWindow(hwnd);
MyBar.SyncTaskbar = true;
```

From this point any change to `Value` or `ProgressState` is automatically reflected
in the Windows taskbar button.

---

## Themes

BetterProgressBar ships with three predefined visual themes that replicate the classic Windows progress bar looks.
Set the `Theme` property to activate one; individual appearance properties can still override specific values afterwards.

```xml
<!-- Windows 11 (default) — slim 4 px bar, rounded corners, accent blue -->
<ctrl:BetterProgressBar Value="60" Theme="Windows11" />

<!-- Windows 10 — 20 px tall, green fill, flat track, no rounding -->
<ctrl:BetterProgressBar Value="60" Theme="Windows10" />

<!-- Windows 7 — 20 px tall, Aero-style gradient fill, glass highlight,
     sunken track, animated shimmer -->
<ctrl:BetterProgressBar Value="60" Theme="Windows7" />

<!-- No theme — full manual control -->
<ctrl:BetterProgressBar Value="60"
                        Theme="None"
                        BarHeight="12"
                        FillBrush="#FF008080"
                        TrackBrush="#FFDDDDDD" />
```

### `ProgressBarTheme` enum

| Value | Description |
|---|---|
| `Windows11` | Slim 4 px bar, rounded corners, accent blue fill *(default)* |
| `Windows10` | 20 px tall, green fill `#06B025`, light gray track, no rounding, shimmer animation |
| `Windows7` | 20 px tall, Aero gradient fill, glass highlight, sunken track gradient, shimmer animation |
| `None` | No preset — all appearance properties are controlled manually |

---

## All five states

```xml
<ctrl:BetterProgressBar Value="65" ProgressState="Normal" />
<ctrl:BetterProgressBar Value="45" ProgressState="Warning" />
<ctrl:BetterProgressBar Value="80" ProgressState="Error" />
<ctrl:BetterProgressBar Value="50" ProgressState="Disabled" />
<ctrl:BetterProgressBar            ProgressState="Indeterminate" />
```

### `ProgressBarState` enum

| Value | Taskbar equivalent | Default color |
|---|---|---|
| `Normal` | `TBPF_NORMAL` | Accent blue (customizable via `FillBrush`) |
| `Warning` | `TBPF_PAUSED` | Orange (customizable via `WarningBrush`) |
| `Error` | `TBPF_ERROR` | Red (customizable via `ErrorBrush`) |
| `Disabled` | `TBPF_NOPROGRESS` | Gray (customizable via `DisabledBrush`) |
| `Indeterminate` | `TBPF_INDETERMINATE` | Animated accent blue |

---

## Tick marks

```xml
<!-- Ticks above only, 10 equal divisions -->
<ctrl:BetterProgressBar Value="40" Ticks="Above" TickFrequency="10" />

<!-- Ticks on both sides, 5 divisions, with percentage label -->
<ctrl:BetterProgressBar Value="75"
                        Ticks="Both"
                        TickFrequency="5"
                        ShowPercentage="True" />
```

`TickFrequency` sets the number of equal divisions (1–10). Endpoint ticks (0 % and 100 %) are tallest,
the midpoint tick (50 %) is medium height, and all intermediate ticks are the shortest.

---

## FillThickness

`FillThickness` lets you create a thin fill stripe inside a taller track:

```xml
<!-- 20 px tall track, 4 px fill stripe centered inside it -->
<ctrl:BetterProgressBar Value="55"
                        Theme="None"
                        BarHeight="20"
                        FillThickness="4" />
```

When `FillThickness` is `0` (default) the fill occupies the full `BarHeight`.

---

## Properties reference

### Range

| Property | Type | Default | Description |
|---|---|---|---|
| `Minimum` | `double` | `0` | Minimum value |
| `Maximum` | `double` | `100` | Maximum value |
| `Value` | `double` | `0` | Current value (coerced to [Minimum, Maximum]) |

### State

| Property | Type | Default | Description |
|---|---|---|---|
| `ProgressState` | `ProgressBarState` | `Normal` | Visual and functional state |
| `IsIndeterminate` | `bool` | `false` | Shortcut — syncs with `ProgressState` |

### Theme

| Property | Type | Default | Description |
|---|---|---|---|
| `Theme` | `ProgressBarTheme` | `Windows11` | Predefined visual preset |

### Taskbar

| Property | Type | Default | Description |
|---|---|---|---|
| `SyncTaskbar` | `bool` | `false` | Enable taskbar progress sync |

```csharp
void SetTaskbarOwnerWindow(nint hwnd)
```

### Tick marks

| Property | Type | Default | Description |
|---|---|---|---|
| `Ticks` | `TickPlacement` | `None` | `None` / `Above` / `Below` / `Both` |
| `TickFrequency` | `double` | `10` | Number of equal divisions (1–10) |

### Percentage label

| Property | Type | Default | Description |
|---|---|---|---|
| `ShowPercentage` | `bool` | `false` | Show percentage to the right of the bar |

### Appearance

| Property | Type | Default | Description |
|---|---|---|---|
| `BarHeight` | `double` | `8` | Height of the track in pixels |
| `FillThickness` | `double` | `0` | Height of the fill stripe (0 = full BarHeight) |
| `FillBrush` | `Brush?` | System accent | Normal-state fill color |
| `TrackBrush` | `Brush?` | Light gray | Track background color |
| `WarningBrush` | `Brush?` | Orange | Fill color for Warning state |
| `ErrorBrush` | `Brush?` | Red | Fill color for Error state |
| `DisabledBrush` | `Brush?` | Gray | Fill color for Disabled state |
| `TickBrush` | `Brush?` | Gray | Tick mark color |

---

## Events

```csharp
MyBar.ValueChanged += (sender, e) =>
{
    Debug.WriteLine($"Value changed: {e.OldValue} → {e.NewValue}");
};
```

---

## Full customization example

```xml
<ctrl:BetterProgressBar Value="55"
                        Theme="None"
                        BarHeight="16"
                        FillThickness="10"
                        Ticks="Both"
                        TickFrequency="5"
                        ShowPercentage="True"
                        FillBrush="#FF008080"
                        TrackBrush="#FFFFE0F0"
                        WarningBrush="#FFFF6600"
                        ErrorBrush="#FFB00020"
                        TickBrush="#FF606060" />
```

---

## License

MIT © [ZipGenius](https://www.zipgenius.it)
