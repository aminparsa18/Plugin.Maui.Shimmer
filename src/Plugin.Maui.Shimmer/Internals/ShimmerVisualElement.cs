namespace Plugin.Maui.Shimmer.Internals;

/// <summary>
/// A flattened, root-relative snapshot of one leaf view inside the shimmered
/// <see cref="ShimmerLayout.PackedView"/> tree. Built once per <see cref="ShimmerLayout.IsLoading"/>
/// transition by <see cref="ViewTreeExtensions"/> and reused by <see cref="ShimmerDrawable"/> on
/// every animation frame, so drawing never re-walks the live view tree.
/// </summary>
internal class ShimmerVisualElement
{
    /// <summary>
    /// At construction, <see cref="X"/>/<see cref="Y"/> are relative to <see cref="Parent"/> — same
    /// as the source view's own <c>X</c>/<c>Y</c>. <see cref="ShimmerLayout"/> resolves them to
    /// root-relative coordinates in place (via <see cref="ViewTreeExtensions.GetAbsoluteX"/>/
    /// <see cref="ViewTreeExtensions.GetAbsoluteY"/>) once flattening is done and the parent chain
    /// is no longer needed, so <see cref="ShimmerDrawable"/> can read them directly.
    /// </summary>
    public float X { get; internal set; }
    public float Y { get; internal set; }
    public float Width { get; }
    public float Height { get; }

    /// <summary>The layout node this element was found under, or <see langword="null"/> at the root.</summary>
    public ShimmerLayoutElement? Parent { get; set; }

    /// <summary>Resolved once when the element is flattened: auto-detected from the source view
    /// (<c>BoxView.CornerRadius</c>, a <c>Border</c>'s <c>RoundRectangle</c> shape) unless overridden
    /// by <see cref="ShimmerLayout.CornerRadiusOverlayProperty"/> or
    /// <see cref="ShimmerLayout.CornerRadiusOverlayDefaultProperty"/>.</summary>
    public CornerRadius CornerRadius { get; set; }

    /// <summary>Resolved once when the element is flattened, from
    /// <see cref="ShimmerLayout.PaddingOverlayProperty"/> or <see cref="ShimmerLayout.PaddingOverlayDefaultProperty"/>.</summary>
    public Thickness Padding { get; set; }

    /// <summary>The view this element was captured from — kept only so overlay attached properties can be read.</summary>
    public View OriginalView { get; }

    public ShimmerVisualElement(float x, float y, float width, float height, View originalView)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        OriginalView = originalView;
    }
}
