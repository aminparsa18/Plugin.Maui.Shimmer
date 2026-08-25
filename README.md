# Plugin.Maui.Shimmer

A shimmer/skeleton-loading control for .NET MAUI, drawn entirely with
`Microsoft.Maui.Graphics` (a `GraphicsView` overlay) — no third-party
rendering dependency.

![Plugin.Maui.Shimmer demo](https://raw.githubusercontent.com/aminparsa18/Plugin.Maui.Shimmer/main/demo.gif)

## How it's meant to be used

`ShimmerLayout` wraps **placeholder shapes** — plain `BoxView`s and
`Border`s standing in for a mimic of an avatar or a line of text or a card — and sweeps
the shimmer over them while data is loading.

```xml
<Grid>
    <!-- Skeleton: only placeholder BoxViews live inside the ShimmerLayout. -->
    <shimmer:ShimmerLayout x:Name="Shimmer" IsLoading="True">
        <VerticalStackLayout Spacing="12">
            <Border StrokeShape="RoundRectangle 18"
                    shimmer:ShimmerLayout.IsContainer="True">
                <Grid ColumnDefinitions="56,*" ColumnSpacing="12">
                    <BoxView WidthRequest="56" HeightRequest="56" CornerRadius="28" />
                    <VerticalStackLayout Grid.Column="1" Spacing="8" VerticalOptions="Center">
                        <BoxView HeightRequest="14" CornerRadius="7" WidthRequest="140" />
                        <BoxView HeightRequest="11" CornerRadius="6" WidthRequest="190" />
                    </VerticalStackLayout>
                </Grid>
            </Border>
        </VerticalStackLayout>
    </shimmer:ShimmerLayout>

</Grid>
```

See [example/Plugin.Maui.Shimmer.Example/MainPage.xaml](example/Plugin.Maui.Shimmer.Example/MainPage.xaml)
for the full working demo above.

### Bindable properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsLoading` | `bool` | `false` | Starts/stops the sweep animation. |
| `PackedView` | `View` | — | Content property — the skeleton tree to shimmer over. |
| `Duration` | `uint` | `1000` | Length of one sweep, in milliseconds. |
| `BackgroundGradientColor` | `Color` | `#B1AEB2` | Resting color of the placeholder shapes. |
| `ForegroundGradientColor` | `Color` | `#9B969C` | Color of the sweeping highlight band. |
| `GradientSize` | `float` | `0.4` | Width of the highlight band, as a fraction of the layout's width. |
| `Angle` | `int` | `-45` | Direction of the sweep, in degrees (CSS gradient-angle convention). |
| `CornerRadiusOverlayDefault` | `CornerRadius` | `0` | Fallback corner radius for children that don't set `CornerRadiusOverlay`. |
| `PaddingOverlayDefault` | `Thickness` | `0` | Fallback padding for children that don't set `PaddingOverlay`. |

### Attached properties

| Property | Applies to | Description |
|---|---|---|
| `ShimmerLayout.IsContainer` | `Border` | A `Border` isn't a `Layout`, so by default it's captured as one shape covering its whole bounds — right for something the shimmer itself should represent (a rounded avatar), wrong for a card wrapping several placeholders. Set `True` to flatten the shimmer into the `Border`'s `Content` instead. |
| `ShimmerLayout.CornerRadiusOverlay` | any child | Overrides the corner radius drawn for that element, in place of whatever was auto-detected (`BoxView.CornerRadius`, a `Border`'s `RoundRectangle`). |
| `ShimmerLayout.PaddingOverlay` | any child | Grows or shrinks the drawn shimmer rect relative to that child's own bounds. |

## Alternatives

[Syncfusion's MAUI Toolkit ships a Shimmer view](https://help.syncfusion.com/maui-toolkit/shimmer/overview)
too, and it's a reasonable option if you're already pulling in the toolkit
for other components. Two differences worth knowing before you choose:

- It's part of the whole Syncfusion MAUI Toolkit — adopting it for one
  control means taking the dependency on the full toolkit.
- Its shapes come from a fixed set of built-in presets.

Plugin.Maui.Shimmer has no dependency beyond `Microsoft.Maui.Controls`,
and there's no preset system: you build the skeleton out of your own
layouts and `BoxView`/`Border` shapes, so it matches whatever you're
loading exactly rather than the closest built-in shape.

## Project structure

```
src/Plugin.Maui.Shimmer/         the library (net10.0-android, net10.0-ios)
  ShimmerLayout.cs                the ShimmerLayout control
  Internals/                      drawable, layout-flattening, and extension helpers
  Handlers/                       shared handler partials go here
  Platforms/
    iOS/Handlers/                 iOS-specific handler implementation
    Android/Handlers/             Android-specific handler implementation
example/Plugin.Maui.Shimmer.Example/   a MAUI app demonstrating the skeleton/real-content pattern above
```

## Getting started

```bash
dotnet add package Plugin.Maui.Shimmer
```

Or open `Plugin.Maui.Shimmer.slnx` and run the `Plugin.Maui.Shimmer.Example`
project.

## License

MIT — see [LICENSE](LICENSE).
