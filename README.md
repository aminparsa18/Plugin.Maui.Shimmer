# Plugin.Maui.Shimmer

A shimmer/skeleton-loading control for .NET MAUI.

> **Status:** scaffolding only. The library and example app are wired up and
> build as empty shells — the actual shimmer implementation hasn't been added
> yet.

## Project structure

```
src/Plugin.Maui.Shimmer/         the library (net10.0-android, net10.0-ios)
  Handlers/                      shared handler partials go here
  Platforms/
    iOS/Handlers/                iOS-specific handler implementation
    Android/Handlers/            Android-specific handler implementation
example/Plugin.Maui.Shimmer.Example/   a minimal MAUI app referencing the library
```

## Getting started

Open `Plugin.Maui.Shimmer.slnx` and run the `Plugin.Maui.Shimmer.Example`
project.

## License

MIT — see [LICENSE](LICENSE).
