# Changelog

All notable changes to **BetterProgressBar.WinUI3** will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
This project adheres to [Semantic Versioning](https://semver.org/).

---

## [0.6.0] – 2025-XX-XX

### Added

- **Icon** – a purpose-designed SVG icon (blue gradient rounded square with a partial-fill
  progress bar and tick marks) is now included in `Graphics/` in five sizes: 256, 128, 64, 32
  and 16 px. A PNG render at 256 px is embedded as the NuGet package icon.
- **README expanded** – full Theme section with code samples for all three themes; all-five-states
  example; `FillThickness` example; complete properties reference table; events example.

### Changed

- NuGet `<Description>` and `<PackageTags>` updated to mention predefined themes.
- **Version bump** to 0.6.0.

---

## [0.5.1] – 2025-XX-XX

### Changed

- **Code cleanup** – removed all inline reasoning comments and intermediate
  notes from source files. Code is now documented with clean XML doc comments
  and concise inline comments only.
- **Version bump** to 0.5.1.

---

## [0.5.0] – 2025-XX-XX

### Added / Changed

- **Windows 7 theme**: all visual layers now match the reference CSS spec exactly —
  track gradient, fill gradient with `BlendOver` compositing, glass highlight (top 40%),
  radial-approximated vignette at both ends, and shimmer animation (3s cycle: 1.8s sweep + 1.2s hold).
- **Windows 10 theme**: confirmed 0 px corner radius on track, fill rect and fill container.
- **Version bump** to 0.5.0.

---

## [0.4.1] – 2025-XX-XX

### Fixed

- **Shimmer restart bug** – the shimmer animation was being restarted on every `Value` or state change
  because `UpdateShimmer` unconditionally enqueued `StartShimmer` via `DispatcherQueue`. Replaced the
  shimmer/decoration scheduling with a single `ScheduleDecoration()` / `RunDecoration()` pipeline that
  collapses multiple rapid calls into one deferred execution. A `_shimmerScheduled` guard flag prevents
  duplicate enqueues; `StartShimmer` is only called when `_shimmerStoryboard is null`.
- **Windows 7 decoration not visible** – `DrawGlassHighlight` and `DrawWin7Stripes` were being
  called synchronously before layout completed, so `ActualWidth`/`ActualHeight` were both 0.
  All decoration now runs inside the single deferred `RunDecoration()` call. A `_lastDecoratedTheme`
  field avoids redundant redraws when the theme has not changed.

---

## [0.4.0] – 2025-XX-XX

### Added

- **`ProgressBarTheme.Windows7`** – 20 px tall bar with green fill, subtle diagonal stripe overlay,
  a glassy top-highlight (semi-transparent white gradient in the upper 45 % of the fill), and the
  same sweeping shimmer animation as the Windows 10 theme. Corner radius: 2 px.
- **Demo** updated with a full Windows 7 theme section (all five states + percentage label).

---

## [0.3.0] – 2025-XX-XX

### Added

- **`Theme` property** (`ProgressBarTheme` enum) – applies a predefined visual preset:
  - `Windows11` *(default)* – 4 px slim bar, rounded corners, accent blue fill, neutral gray track.
  - `Windows10` – 20 px tall bar, green fill (`#06B025`), light gray track (`#C8C8C8`), slight rounding.
  - `None` – no preset; all appearance properties keep their explicitly set values.
- Setting `Theme` applies `BarHeight`, `FillThickness`, `FillBrush`, `TrackBrush`, and corner radius
  simultaneously. Individual properties can still be overridden after the theme is applied.
- **Demo** updated to showcase both themes across all five states.

### Changed

- Fill is now rendered via a `PART_FillRect` (`Rectangle`) instead of the inner WinUI `ProgressBar`,
  eliminating the `MinHeight` constraint that previously prevented `FillThickness` from working.
  The inner `ProgressBar` is retained but used exclusively for the indeterminate animation.
- Endpoint and midpoint tick stroke reduced from 3 px → 2 px.
- Version bump to 0.3.0.

---

## [0.2.3] – 2025-XX-XX

### Added

- **Demo**: Windows 10–style example (20 px height, green fill `#FF06B025`, light gray track
  `#FFC8C8C8`), correctly matching the actual Windows 10 progress bar appearance.

### Changed

- **Full English translation** – all Italian strings in source code comments, XML documentation,
  XAML labels, README, and CHANGELOG have been translated to English.
- **Version bump** to 0.2.3.

---

## [0.2.1] – 2025-XX-XX

### Fixed

- **Tick mark alignment** – `DrawTicks` now uses `canvas.ActualWidth` instead of the full control
  `ActualWidth`, so tick marks align correctly with the bar when the percentage label is visible.

### Added

- **Demo**: Windows 10–style example (2 px height, flat track, accent color fill).

---

## [0.2.0] – 2025-XX-XX

### Added

- **`FillThickness` property** – controls the height of the fill stripe inside the track
  independently from `BarHeight`. When `0` (default) the fill occupies the full `BarHeight`.
  Values greater than `BarHeight` are clamped automatically.
- **`TickFrequency` is now a division count (1–10)** – represents the number of equal divisions
  of the range (e.g. `10` = a tick every 10 %). Values are clamped to `[1, 10]`.

### Changed

- **Percentage label moved outside the bar** – the `ShowPercentage` label is now displayed to
  the right of the bar, vertically centered. Its color follows the system foreground
  (`Foreground` template binding).
- **Tick marks improved** – endpoints (0 %, 100 %) are 12 px tall, midpoint (50 %) is 8 px,
  intermediate ticks are 5 px. Ticks above the bar grow downward from the canvas bottom edge
  for correct visual alignment when `Ticks = Both`.
- **Bar row centered correctly** – the track row uses `Height="*"` and `VerticalAlignment="Center"`
  so the track is always perfectly centred between the tick canvases.
- **Demo app** set as startup project in the solution.
- **Packages updated**: `Microsoft.WindowsAppSDK` → `1.8.260209005`,
  `Microsoft.Windows.SDK.BuildTools` → `10.0.26100.7705`.

---

## [0.1.0] – 2025-XX-XX

### Added

- **Core control** – `BetterProgressBar` custom WinUI 3 control (`ZipGenius.BetterProgressBar` namespace).
- **Full `ProgressBar` API parity** – `Minimum`, `Maximum`, `Value` (with coercion), `IsIndeterminate`,
  `ValueChanged` event.
- **`ProgressState` property** – five named states: `Normal`, `Warning`, `Error`, `Disabled`,
  `Indeterminate`.
- **Taskbar progress integration** via `ITaskbarList3` COM interop (`SetTaskbarOwnerWindow`,
  `SyncTaskbar`).
- **Tick marks** via `Ticks` (`TickPlacement` enum: `None`, `Above`, `Below`, `Both`).
- **Percentage label** (`ShowPercentage` property).
- **Appearance customization**: `BarHeight`, `FillBrush`, `TrackBrush`, `WarningBrush`,
  `ErrorBrush`, `DisabledBrush`, `TickBrush`.
- **Multi-TFM support**: `net8.0`, `net9.0`, `net10.0` targeting `windows10.0.19041.0`.
- **CI/CD** – GitHub Actions workflows for build validation and NuGet publishing on version tags.
