using Plugin.Maui.Shimmer.Internals;

namespace Plugin.Maui.Shimmer;

/// <inheritdoc />
/// <summary>
/// Adds a shimmering/skeleton-loading effect over every child element of <see cref="PackedView"/>.
/// Drawn entirely with <c>Microsoft.Maui.Graphics</c> (a <see cref="GraphicsView"/> overlay) — no
/// third-party rendering dependency.
/// </summary>
[ContentProperty(nameof(PackedView))]
public class ShimmerLayout : Grid
{
    #region Bindable Properties

    /// <summary>Backing store for <see cref="IsLoading"/>.</summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading), typeof(bool), typeof(ShimmerLayout), false,
        propertyChanged: (b, _, _) => ((ShimmerLayout)b).Invalidate());

    /// <summary>The IsLoading Property to Enable/Disable The Shimmer</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Backing store for <see cref="Duration"/>.</summary>
    public static readonly BindableProperty DurationProperty = BindableProperty.Create(
        nameof(Duration), typeof(uint), typeof(ShimmerLayout), 1000U);

    /// <summary>The Duration of one shimmer sweep, in milliseconds.</summary>
    public uint Duration
    {
        get => (uint)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Backing store for <see cref="PackedView"/>.</summary>
    public static readonly BindableProperty PackedViewProperty = BindableProperty.Create(
        nameof(PackedView), typeof(View), typeof(ShimmerLayout),
        propertyChanged: (b, o, n) => ((ShimmerLayout)b).UpdatePackedView((View)o, (View)n));

    /// <summary>The View we want to apply the Shimmer to.</summary>
    public View PackedView
    {
        get => (View)GetValue(PackedViewProperty);
        set => SetValue(PackedViewProperty, value);
    }

    /// <summary>Backing store for <see cref="BackgroundGradientColor"/>.</summary>
    public static readonly BindableProperty BackgroundGradientColorProperty = BindableProperty.Create(
        nameof(BackgroundGradientColor), typeof(Color), typeof(ShimmerLayout), Color.FromArgb("#B1AEB2"));

    /// <summary>The base (resting) color of the shimmer.</summary>
    public Color BackgroundGradientColor
    {
        get => (Color)GetValue(BackgroundGradientColorProperty);
        set => SetValue(BackgroundGradientColorProperty, value);
    }

    /// <summary>Backing store for <see cref="ForegroundGradientColor"/>.</summary>
    public static readonly BindableProperty ForegroundGradientColorProperty = BindableProperty.Create(
        nameof(ForegroundGradientColor), typeof(Color), typeof(ShimmerLayout), Color.FromArgb("#9B969C"));

    /// <summary>The color of the sweeping highlight band.</summary>
    public Color ForegroundGradientColor
    {
        get => (Color)GetValue(ForegroundGradientColorProperty);
        set => SetValue(ForegroundGradientColorProperty, value);
    }

    /// <summary>Backing store for <see cref="GradientSize"/>.</summary>
    public static readonly BindableProperty GradientSizeProperty = BindableProperty.Create(
        nameof(GradientSize), typeof(float), typeof(ShimmerLayout), 0.4f);

    /// <summary>Width of the sweeping highlight band, as a fraction of the layout's own width.</summary>
    public float GradientSize
    {
        get => (float)GetValue(GradientSizeProperty);
        set => SetValue(GradientSizeProperty, value);
    }

    /// <summary>Backing store for <see cref="Angle"/>.</summary>
    public static readonly BindableProperty AngleProperty = BindableProperty.Create(
        nameof(Angle), typeof(int), typeof(ShimmerLayout), -45);

    /// <summary>Direction of the gradient band, in degrees (CSS gradient-angle convention).</summary>
    public int Angle
    {
        get => (int)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <summary>Backing store for <see cref="CornerRadiusOverlayDefault"/>.</summary>
    public static readonly BindableProperty CornerRadiusOverlayDefaultProperty = BindableProperty.Create(
        nameof(CornerRadiusOverlayDefault), typeof(CornerRadius), typeof(ShimmerLayout), default(CornerRadius));

    /// <summary>Fallback corner radius used for any child that doesn't set the
    /// <see cref="CornerRadiusOverlayProperty"/> attached property itself.</summary>
    public CornerRadius CornerRadiusOverlayDefault
    {
        get => (CornerRadius)GetValue(CornerRadiusOverlayDefaultProperty);
        set => SetValue(CornerRadiusOverlayDefaultProperty, value);
    }

    /// <summary>Backing store for <see cref="PaddingOverlayDefault"/>.</summary>
    public static readonly BindableProperty PaddingOverlayDefaultProperty = BindableProperty.Create(
        nameof(PaddingOverlayDefault), typeof(Thickness), typeof(ShimmerLayout), default(Thickness));

    /// <summary>Fallback padding used for any child that doesn't set the
    /// <see cref="PaddingOverlayProperty"/> attached property itself.</summary>
    public Thickness PaddingOverlayDefault
    {
        get => (Thickness)GetValue(PaddingOverlayDefaultProperty);
        set => SetValue(PaddingOverlayDefaultProperty, value);
    }

    #endregion

    #region Attached Properties

    /// <summary>Backing store for the <c>CornerRadiusOverlay</c> attached property.</summary>
    public static readonly BindableProperty CornerRadiusOverlayProperty = BindableProperty.CreateAttached(
        "CornerRadiusOverlay", typeof(CornerRadius), typeof(ShimmerLayout), default(CornerRadius));

    /// <summary>Per-child override for the shimmer rect's corner radius, drawn in place of whatever
    /// was auto-detected from the child's own type (<c>BoxView.CornerRadius</c>, a <c>Border</c>'s
    /// <c>RoundRectangle</c>).</summary>
    public static CornerRadius GetCornerRadiusOverlay(BindableObject view) => (CornerRadius)view.GetValue(CornerRadiusOverlayProperty);

    /// <summary>Sets the per-child corner radius override — see <see cref="GetCornerRadiusOverlay"/>.</summary>
    public static void SetCornerRadiusOverlay(BindableObject view, CornerRadius value) => view.SetValue(CornerRadiusOverlayProperty, value);

    /// <summary>Backing store for the <c>PaddingOverlay</c> attached property.</summary>
    public static readonly BindableProperty PaddingOverlayProperty = BindableProperty.CreateAttached(
        "PaddingOverlay", typeof(Thickness), typeof(ShimmerLayout), default(Thickness));

    /// <summary>Per-child override that grows or shrinks the drawn shimmer rect relative to the
    /// child's own bounds.</summary>
    public static Thickness GetPaddingOverlay(BindableObject view) => (Thickness)view.GetValue(PaddingOverlayProperty);

    /// <summary>Sets the per-child padding override — see <see cref="GetPaddingOverlay"/>.</summary>
    public static void SetPaddingOverlay(BindableObject view, Thickness value) => view.SetValue(PaddingOverlayProperty, value);

    /// <summary>Backing store for the <c>IsContainer</c> attached property.</summary>
    public static readonly BindableProperty IsContainerProperty = BindableProperty.CreateAttached(
        "IsContainer", typeof(bool), typeof(ShimmerLayout), false);

    /// <summary>Marks a <c>Border</c> as decorative chrome rather than a shimmer target. A
    /// <c>Border</c> isn't a <see cref="Microsoft.Maui.Controls.Layout"/>, so by default it's
    /// captured as a single leaf — one shape drawn over its whole bounds, same as a plain
    /// <c>BoxView</c> (see <see cref="CornerRadiusOverlayProperty"/>). That's right for something the
    /// shimmer itself should represent, like a rounded avatar. It's wrong for a card-style
    /// <c>Border</c> wrapping real content (an avatar, a couple of labels) — set this to
    /// <see langword="true"/> on that <c>Border</c> and the shimmer flattens into its
    /// <c>Content</c> instead, so the card's own background/shadow stays fully visible while
    /// what's inside it shimmers individually.</summary>
    public static bool GetIsContainer(BindableObject view) => (bool)view.GetValue(IsContainerProperty);

    /// <summary>Sets the per-<c>Border</c> container flag — see <see cref="GetIsContainer"/>.</summary>
    public static void SetIsContainer(BindableObject view, bool value) => view.SetValue(IsContainerProperty, value);

    #endregion

    private const string ShimmerAnimationName = "ShimmerAnimation";

    private bool _isSizeAllocated;
    private GraphicsView? _maskGraphicsView;
    private ShimmerDrawable? _drawable;
    private CancellationTokenSource? _animationCancellationTokenSource;
    private TaskCompletionSource<bool>? _animationCycleCompletionSource;

    /// <summary>Creates a <see cref="ShimmerLayout"/> with the default shimmer settings.</summary>
    public ShimmerLayout()
    {
        IsClippedToBounds = true;

        SizeChanged += OnElementSizeChanged;
    }

    #region Events

    /// <summary>We want Width/Height to be known before the first shimmer pass — fires once, then
    /// unhooks itself; later resizes take effect on the next <see cref="IsLoading"/> toggle.</summary>
    private void OnElementSizeChanged(object? sender, EventArgs args)
    {
        SizeChanged -= OnElementSizeChanged;

        _isSizeAllocated = true;

        Invalidate();
    }

    /// <inheritdoc />
    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Removed from the visual tree — stop the animation loop rather than leaving it running
        // forever against a detached GraphicsView.
        if (Parent is null)
            CancelAnimation();
    }

    #endregion

    #region Property Changed

    private void Invalidate()
    {
        if (!_isSizeAllocated) return;

        if (IsLoading) ApplyShimmer();
        else RemoveShimmer();
    }

    private void UpdatePackedView(View? oldValue, View? newValue)
    {
        if (oldValue != null && Children.Contains(oldValue))
            Children.Remove(oldValue);

        if (newValue != null)
            Children.Insert(0, newValue);
    }

    #endregion

    #region Shimmer Animation

    /// <summary>Builds (or reuses) the overlay <see cref="GraphicsView"/>, snapshots the current
    /// <see cref="PackedView"/> tree, and starts the sweep loop.</summary>
    private void ApplyShimmer()
    {
        if (_maskGraphicsView is null)
        {
            _drawable = new ShimmerDrawable();
            _maskGraphicsView = new GraphicsView
            {
                Drawable = _drawable,
                Opacity = 0,
                IsVisible = false,
                InputTransparent = true,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
            };

            Children.Add(_maskGraphicsView);
        }

        UpdateGradient();

        // Deferred one dispatcher tick: SizeChanged on *this* layout fires as soon as its own
        // bounds are set — before it has arranged its own children (PackedView's tree). Reading a
        // child BoxView's Width/Height synchronously from here catches it mid-layout, before
        // Arrange has run on it, so MAUI hands back -1 (its "not yet measured" sentinel) instead of
        // the real size — ExtractVisualElements would then bake a degenerate near-zero rect into
        // every element, which paints as an invisible sliver pinned at the corner. Dispatching lets
        // the current layout pass (including all descendants) finish first.
        Dispatcher.Dispatch(() =>
        {
            ExtractVisualElements();

            // Fire-and-forget, but deliberately *not* Task.Run: StartAnimation drives a
            // Microsoft.Maui.Controls.Animation, whose Ticker is wired to the platform's main-thread
            // frame clock (CADisplayLink/Choreographer/CompositionTarget). Committing and ticking it
            // from a thread-pool thread starves/drops frames unpredictably — the sweep reads as a
            // weak pulse instead of a smooth transit. async/await already yields the calling thread
            // at each await, so there's no need to hop off it to begin with.
            _ = StartAnimation();
        });
    }

    /// <summary>Resolves the current gradient colors and the (fixed, for the whole cycle) sweep
    /// direction from <see cref="Angle"/> — stable for the whole shimmer cycle, so this only runs
    /// once per <see cref="ApplyShimmer"/> call rather than every frame.</summary>
    private void UpdateGradient()
    {
        if (_drawable is null) return;

        _drawable.BackgroundColor = BackgroundGradientColor;
        _drawable.ForegroundColor = ForegroundGradientColor;
        _drawable.WaveWidthFraction = GradientSize;

        var (start, end) = ((double)Angle).ToGradientPoints();
        _drawable.DirectionStart = start;
        _drawable.DirectionEnd = end;
    }

    /// <summary>Flattens <see cref="PackedView"/> into leaf elements with root-relative coordinates
    /// and a resolved corner radius/padding, ready for <see cref="ShimmerDrawable"/> to paint every
    /// frame without touching the live view tree.</summary>
    private void ExtractVisualElements()
    {
        if (_drawable is null || PackedView is null) return;

        var leaves = PackedView is Layout packedLayout
            ? packedLayout.ToShimmerLayoutElement().Flatten().ToList()
            : [PackedView.ToShimmerVisualElement()];

        foreach (var element in leaves)
        {
            // Root-relative coordinates — done once here, while the parent chain is still attached,
            // rather than on every animation frame.
            element.X = element.GetAbsoluteX();
            element.Y = element.GetAbsoluteY();

            var corner = GetCornerRadiusOverlay(element.OriginalView);
            var padding = GetPaddingOverlay(element.OriginalView);

            if (corner == default) corner = CornerRadiusOverlayDefault;
            if (padding == default) padding = PaddingOverlayDefault;

            // Only replace the auto-detected corner radius (from BoxView/Border) when an override
            // was actually configured somewhere — otherwise leave the shape's own rounding alone.
            if (corner != default || CornerRadiusOverlayDefault != default)
                element.CornerRadius = corner;

            element.Padding = padding;
        }

        _drawable.Elements = leaves;
    }

    /// <summary>Fades the overlay in, then loops a sweep animation until <see cref="RemoveShimmer"/>
    /// cancels it. Must run on the main thread throughout — see the call site in
    /// <see cref="ApplyShimmer"/> for why.</summary>
    private async Task StartAnimation()
    {
        CancelAnimation();

        if (_maskGraphicsView is not { } graphicsView || _drawable is not { } drawable) return;

        graphicsView.Opacity = 0;
        graphicsView.IsVisible = true;

        await Task.WhenAll(
            graphicsView.FadeToAsync(1, 250U, Easing.Linear),
            RunSweepLoop(graphicsView, drawable));
    }

    /// <summary>Repeatedly commits one sweep <see cref="Animation"/> — <see cref="ShimmerDrawable.Progress"/>
    /// from <c>0</c> to <c>1 + GradientSize</c> — until <see cref="CancelAnimation"/> requests
    /// cancellation. Deliberately not <c>Task.Run</c> — see <see cref="ApplyShimmer"/>.</summary>
    private async Task RunSweepLoop(GraphicsView graphicsView, ShimmerDrawable drawable)
    {
        _animationCancellationTokenSource = new CancellationTokenSource();
        var token = _animationCancellationTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            // Captured locally (not just assigned to the field) so the Commit callback below always
            // completes *this* cycle's TCS — if a stale cycle's abort-finished callback fires late,
            // after a newer StartAnimation() has already replaced the field, it must not resolve the
            // new cycle's TCS instead and cut its sweep short.
            var cycleCompletion = new TaskCompletionSource<bool>();
            _animationCycleCompletionSource = cycleCompletion;

            new Animation
            {
                {
                    0, 1,
                    new Animation(t =>
                    {
                        drawable.Progress = (float)t;
                        graphicsView.Invalidate();
                    }, 0, 1 + GradientSize)
                }
            }.Commit(graphicsView, ShimmerAnimationName, 16, Duration, Easing.Linear,
                (_, c) => cycleCompletion.TrySetResult(c));

            await cycleCompletion.Task;
        }
    }

    private void CancelAnimation()
    {
        _animationCancellationTokenSource?.Cancel();
        _maskGraphicsView?.AbortAnimation(ShimmerAnimationName);
    }

    private void RemoveShimmer()
    {
        CancelAnimation();

        if (_maskGraphicsView is null) return;

        // Same reasoning as ApplyShimmer: FadeToAsync commits a MAUI Animation, so this must stay
        // on the calling (main) thread rather than hop to the thread pool via Task.Run.
        _ = FadeOutAsync(_maskGraphicsView);
    }

    private static async Task FadeOutAsync(GraphicsView graphicsView)
    {
        await graphicsView.FadeToAsync(0, 250U, Easing.Linear);
        graphicsView.IsVisible = false;
    }

    #endregion
}
