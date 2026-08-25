namespace Plugin.Maui.Shimmer.Internals;

/// <summary>
/// A flattened <see cref="Layout"/> node — carries its own children so the tree under
/// <see cref="ShimmerLayout.PackedView"/> can be walked once and reduced to a flat list of leaf
/// <see cref="ShimmerVisualElement"/>s (see <see cref="ViewTreeExtensions.Flatten"/>). Layouts are
/// never drawn themselves — only their leaves are.
/// </summary>
internal sealed class ShimmerLayoutElement : ShimmerVisualElement
{
    public IList<ShimmerVisualElement> Children { get; set; } = new List<ShimmerVisualElement>();

    public ShimmerLayoutElement(float x, float y, float width, float height, View originalView)
        : base(x, y, width, height, originalView)
    {
    }
}
