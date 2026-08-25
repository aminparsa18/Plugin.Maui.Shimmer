namespace Plugin.Maui.Shimmer.Example;

public partial class MainPage : ContentPage
{
    private bool _isLoading;

    public MainPage()
    {
        InitializeComponent();

        // The page opens already shimmering (IsLoading="True" in XAML, RealContent hidden) — run one
        // load cycle immediately so the demo shows the sweep, then the reveal, without any interaction.
        _ = RunLoadCycleAsync();
    }

    private void OnReloadTapped(object? sender, EventArgs e) => _ = RunLoadCycleAsync();

    /// <summary>Shows the skeleton, waits as if fetching data, then swaps to the real content — the
    /// pattern a real screen would follow around an actual network call. ShimmerLayout only ever
    /// wraps the skeleton placeholders; the real data lives in a separate layout (RealContent) that
    /// is shown/hidden alongside it, never inside it.</summary>
    private async Task RunLoadCycleAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        ReloadBorder.Opacity = 0.5;
        ReloadBorder.InputTransparent = true;
        StatusLabel.Text = "Loading…";

        RealContent.Opacity = 0;
        RealContent.IsVisible = false;
        Shimmer.IsVisible = true;
        Shimmer.IsLoading = true;

        await Task.Delay(3000);

        Shimmer.IsLoading = false;
        Shimmer.IsVisible = false;

        RealContent.IsVisible = true;
        await RealContent.FadeTo(1, 250);

        StatusLabel.Text = $"Up to date · {DateTime.Now:t}";
        ReloadBorder.Opacity = 1;
        ReloadBorder.InputTransparent = false;

        _isLoading = false;
    }
}
