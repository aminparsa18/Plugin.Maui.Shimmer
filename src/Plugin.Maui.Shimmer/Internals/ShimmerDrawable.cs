namespace Plugin.Maui.Shimmer.Internals;

/// <summary>
/// Paints one rounded rect per flattened <see cref="ShimmerVisualElement"/>, filled with a linear
/// gradient whose highlight band sweeps across as <see cref="Progress"/> advances. Owned and driven
/// by <see cref="ShimmerLayout"/>, which updates <see cref="Progress"/> and calls
/// <see cref="Microsoft.Maui.Controls.GraphicsView.Invalidate()"/> on every animation tick — this
/// class only knows how to render one frame, not how the animation itself is paced.
/// </summary>
/// <remarks>
/// The gradient's own <see cref="DirectionStart"/>/<see cref="DirectionEnd"/> and the rect handed to
/// <see cref="Microsoft.Maui.Graphics.ICanvas.SetFillPaint(Microsoft.Maui.Graphics.Paint, RectF)"/>
/// stay <b>fixed</b> for the whole sweep — only the three gradient stops' <em>offsets</em> move, from
/// bunched up at 0 (the highlight sits just past the left/top edge) to bunched up at 1 (it's just
/// past the right/bottom edge). Moving the reference rect itself (what the previous implementation
/// did) makes the gradient's Start/End fall outside the shape being filled for most of the sweep;
/// depending on the paint's edge/tile behavior that can render as a static, un-animated fill instead
/// of a moving streak. Offset-only animation against a fixed rect is the same technique
/// Syncfusion's Shimmer control uses and is the robust way to drive a
/// <see cref="Microsoft.Maui.Graphics.LinearGradientPaint"/> sweep in MAUI.
/// </remarks>
internal sealed class ShimmerDrawable : IDrawable
{
    /// <summary>Elements to paint this frame, already root-relative and with corner
    /// radius/padding resolved — see <see cref="ShimmerLayout"/>.</summary>
    /// <remarks>Set once per shimmer cycle by <see cref="ShimmerLayout.ExtractVisualElements"/>, not
    /// on every animation tick, so the geometry is static for the life of a cycle. The setter builds
    /// the combined <see cref="PathF"/> once here rather than <see cref="Draw"/> rebuilding it from
    /// scratch on every frame.</remarks>
    public IReadOnlyList<ShimmerVisualElement> Elements
    {
        get => elements;
        set
        {
            elements = value;

            elementsPath?.Dispose();
            elementsPath = BuildPath(value);
        }
    }

    IReadOnlyList<ShimmerVisualElement> elements = [];
    PathF? elementsPath;

    static PathF BuildPath(IReadOnlyList<ShimmerVisualElement> elements)
    {
        var path = new PathF();
        foreach (var element in elements)
        {
            path.AppendRoundedRectangle(
                new RectF(element.X - (float)element.Padding.Left, element.Y - (float)element.Padding.Top,
                    element.Width + (float)element.Padding.HorizontalThickness,
                    element.Height + (float)element.Padding.VerticalThickness),
                (float)element.CornerRadius.TopLeft,
                (float)element.CornerRadius.TopRight,
                (float)element.CornerRadius.BottomLeft,
                (float)element.CornerRadius.BottomRight);
        }

        return path;
    }

    /// <summary>The base (resting) color, painted everywhere the highlight band isn't currently over.</summary>
    public Color BackgroundColor { get; set; } = Colors.Transparent;

    /// <summary>The color of the sweeping highlight band.</summary>
    public Color ForegroundColor { get; set; } = Colors.Transparent;

    /// <summary>Direction of the gradient, fixed for the whole sweep and derived once per shimmer
    /// cycle from <see cref="ShimmerLayout.Angle"/> — see <see cref="GradientAngleExtensions"/>.</summary>
    public Point DirectionStart { get; set; }
    public Point DirectionEnd { get; set; }

    /// <summary>Width of the highlight band, as a fraction of the shimmer area along the gradient
    /// axis — same unit as <see cref="ShimmerLayout.GradientSize"/>.</summary>
    public float WaveWidthFraction { get; set; }

    /// <summary>Sweep position. Animated by <see cref="ShimmerLayout"/> from <c>0</c> to
    /// <c>1 + WaveWidthFraction</c> each cycle so the band starts just off the leading edge and ends
    /// just past the trailing edge. Values are clamped to [0,1] per stop before painting, so the
    /// caller doesn't need to pre-clamp.</summary>
    public float Progress { get; set; }

    // Reused across frames instead of rebuilding a LinearGradientPaint (and its three
    // PaintGradientStops) on every tick. Only the stop offsets change frame to frame; the stops'
    // colors and the gradient's direction are rebuilt lazily, only when those inputs actually
    // change (e.g. a new shimmer cycle). PaintGradientStop.Offset/Color and GradientPaint's
    // direction are all mutable, and canvas.SetFillPaint resolves the native paint fresh from
    // whatever state this instance holds each call, so mutating in place is safe.
    LinearGradientPaint? gradient;
    PaintGradientStop? leadingStop;
    PaintGradientStop? centerStop;
    PaintGradientStop? trailingStop;
    Color cachedBackgroundColor = Colors.Transparent;
    Color cachedForegroundColor = Colors.Transparent;
    Point cachedDirectionStart;
    Point cachedDirectionEnd;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (elementsPath is not { } path || elements.Count == 0)
            return;

        var trailingEdge = Math.Clamp(Progress, 0f, 1f);
        var leadingEdge = Math.Clamp(Progress - WaveWidthFraction, 0f, 1f);
        var center = Math.Clamp(Progress - WaveWidthFraction / 2f, 0f, 1f);

        if (gradient is null
            || cachedBackgroundColor != BackgroundColor
            || cachedForegroundColor != ForegroundColor
            || cachedDirectionStart != DirectionStart
            || cachedDirectionEnd != DirectionEnd)
        {
            leadingStop = new PaintGradientStop(leadingEdge, BackgroundColor);
            centerStop = new PaintGradientStop(center, ForegroundColor);
            trailingStop = new PaintGradientStop(trailingEdge, BackgroundColor);
            gradient = new LinearGradientPaint([leadingStop, centerStop, trailingStop], DirectionStart, DirectionEnd);

            cachedBackgroundColor = BackgroundColor;
            cachedForegroundColor = ForegroundColor;
            cachedDirectionStart = DirectionStart;
            cachedDirectionEnd = DirectionEnd;
        }
        else
        {
            leadingStop!.Offset = leadingEdge;
            centerStop!.Offset = center;
            trailingStop!.Offset = trailingEdge;
        }

        // Fixed reference rect (the combined bounds of everything we're about to fill) — only the
        // stop offsets above move from frame to frame. See the class remarks.
        canvas.SetFillPaint(gradient, path.Bounds);
        canvas.FillPath(path);
    }
}
