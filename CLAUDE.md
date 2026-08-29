# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`Plugin.Maui.Shimmer` is a .NET MAUI NuGet library: a shimmer/skeleton-loading
control (`ShimmerLayout`) drawn entirely with `Microsoft.Maui.Graphics` (a
`GraphicsView` overlay). No third-party rendering dependency — the only
package reference is `Microsoft.Maui.Controls`.

Two projects, both `net10.0-android;net10.0-ios`:
- `src/Plugin.Maui.Shimmer/` — the library that gets packed and published.
- `example/Plugin.Maui.Shimmer.Example/` — a MAUI app demonstrating the
  skeleton/real-content pattern (`MainPage.xaml` is the canonical usage
  reference).

Open `Plugin.Maui.Shimmer.slnx` to work across both.

## Build / run

```bash
# Build the library
dotnet build src/Plugin.Maui.Shimmer/Plugin.Maui.Shimmer.csproj

# Pack (what publish.yml does)
dotnet pack src/Plugin.Maui.Shimmer/Plugin.Maui.Shimmer.csproj -c Release -o ./nupkg

# Run the example on a connected Android device
scripts/run-android-device.sh <device-serial>          # one-shot build+deploy
scripts/run-android-device.sh --watch <device-serial>   # dotnet watch, streams logs

# Equivalent manual invocation
dotnet build example/Plugin.Maui.Shimmer.Example/Plugin.Maui.Shimmer.Example.csproj \
  -f net10.0-android -t:Run -c Debug -p:AdbTarget="-s <device-serial>"
```

Use `adb devices` to find a serial. There is no test project in this repo.

**Never build/run/deploy the app or drive a device without explicit
go-ahead first** — ask before invoking any of the above.

## Architecture

`ShimmerLayout` (`src/Plugin.Maui.Shimmer/ShimmerLayout.cs`) is a `Grid`
subclass with a `[ContentProperty(nameof(PackedView))]` — consumers wrap
their skeleton (`BoxView`/`Border` placeholders) as its content, and it
overlays one shared `GraphicsView` that paints the sweep on top of them. The
placeholders themselves stay in the visual tree unchanged; only the overlay
animates.

The pipeline, on every `IsLoading` toggle to `true`:

1. **Flatten** (`Internals/ViewTreeExtensions.cs`) — walks the `PackedView`
   tree once and reduces it to root-relative leaf rectangles
   (`ShimmerVisualElement`/`ShimmerLayoutElement` in `Internals/`). Layouts
   are never drawn, only their leaves. A `Border` is captured as one leaf
   *unless* `ShimmerLayout.IsContainer="True"` is set on it, in which case it
   flattens into its `Content` instead (see the attached properties in the
   README). Corner radius is auto-detected from `BoxView.CornerRadius` /
   `Border`'s `RoundRectangle`, then overridable per-child
   (`CornerRadiusOverlay`/`PaddingOverlay` attached properties) or via layout
   defaults (`CornerRadiusOverlayDefault`/`PaddingOverlayDefault`).
2. **Build path once** (`Internals/ShimmerDrawable.cs`) — the flattened
   leaves are combined into a single `PathF` when `Elements` is set, not
   rebuilt every frame.
3. **Animate** — a `Microsoft.Maui.Controls.Animation` drives
   `ShimmerDrawable.Progress` from `0` to `1 + GradientSize` each cycle,
   looping until `IsLoading` goes false. Only the gradient's three stop
   *offsets* move each frame; the reference rect and gradient direction stay
   fixed for the whole sweep (see the remarks in `ShimmerDrawable` for why —
   moving the rect instead can make the paint stop animating visibly).
   `GradientAngleExtensions.ToGradientPoints` converts the CSS-style `Angle`
   degree value into the start/end points that direction needs.

Threading/timing details worth preserving when touching this code (both are
called out at the relevant call sites, don't relitigate them casually):
- `ExtractVisualElements()` runs one dispatcher tick after `SizeChanged`,
  not synchronously — reading a child's size before its own `Arrange` has
  run returns MAUI's `-1` "unmeasured" sentinel.
- The animation loop is deliberately kept on the main thread (no
  `Task.Run`) because `Animation.Commit`'s ticker is wired to the platform
  frame clock; hopping to the thread pool drops frames.

`Handlers/`, `Platforms/iOS/Handlers/`, `Platforms/Android/Handlers/`
currently contain only `.gitkeep` — scaffolding for future custom handlers,
not yet implemented. Everything today is pure `Microsoft.Maui.Graphics`
drawing, no platform-specific code.

## Publishing

`.github/workflows/publish.yml` packs and pushes to nuget.org on GitHub
`release: published` (or manual `workflow_dispatch`), using NuGet Trusted
Publishing (OIDC) — no stored API key. One-time nuget.org/GitHub secret setup
is documented in comments at the top of that file.