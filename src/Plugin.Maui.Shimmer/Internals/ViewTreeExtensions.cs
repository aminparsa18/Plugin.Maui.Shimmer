using Microsoft.Maui.Controls.Shapes;

namespace Plugin.Maui.Shimmer.Internals;

/// <summary>
/// Walks the view tree under <see cref="ShimmerLayout.PackedView"/> and flattens it into
/// root-relative <see cref="ShimmerVisualElement"/> leaves. Layout-agnostic — knows nothing about
/// <see cref="ShimmerLayout"/>'s overlay properties; the caller resolves those afterward.
/// </summary>
internal static class ViewTreeExtensions
{
    /// <summary>Captures a leaf view's position/size and auto-detects a corner radius from
    /// well-known shaped controls, so plain <c>BoxView</c>/<c>Border</c> placeholders shimmer with
    /// their own rounding by default.</summary>
    public static ShimmerVisualElement ToShimmerVisualElement(this View element)
    {
        return new ShimmerVisualElement((float)element.X, (float)element.Y, (float)element.Width, (float)element.Height, element)
        {
            CornerRadius = element switch
            {
                BoxView boxView => boxView.CornerRadius,
                Border { StrokeShape: RoundRectangle roundRectangle } => roundRectangle.CornerRadius,
                _ => default
            }
        };
    }

    /// <summary>Recursively captures a layout and its children, preserving the parent chain so
    /// <see cref="GetAbsoluteX"/>/<see cref="GetAbsoluteY"/> can later resolve root-relative coordinates.</summary>
    public static ShimmerLayoutElement ToShimmerLayoutElement(this Layout layout)
    {
        var layoutElement = new ShimmerLayoutElement((float)layout.X, (float)layout.Y, (float)layout.Width, (float)layout.Height, layout);

        var children = new List<ShimmerVisualElement>();
        foreach (var child in layout.Children)
        {
            if (child is Layout childLayout)
            {
                // The recursive call's own Parent is unset at this point — wired up here, same as
                // the leaf branch below, so a leaf two or more Layouts deep (e.g. a Grid row whose
                // text column is itself a VerticalStackLayout) still resolves every ancestor's
                // offset instead of stopping one level short.
                var childLayoutElement = childLayout.ToShimmerLayoutElement();
                childLayoutElement.Parent = layoutElement;
                children.Add(childLayoutElement);
            }
            else if (child is Border { Content: View borderContent } border && ShimmerLayout.GetIsContainer(border))
            {
                // Marked as decorative chrome (ShimmerLayout.IsContainer="True") rather than a
                // shimmer target: flatten into its Content instead of capturing the Border itself as
                // one leaf, the same way a Layout is descended into just above. Without this, any
                // Border wrapping real content — a card background, a shadow — swallows everything
                // inside it into a single shape, since Border isn't a Layout and would otherwise hit
                // the leaf branch below.
                var containerElement = border.ToShimmerContainerElement(borderContent);
                containerElement.Parent = layoutElement;
                children.Add(containerElement);
            }
            else if (child is View childView)
            {
                var element = childView.ToShimmerVisualElement();
                element.Parent = layoutElement;
                children.Add(element);
            }
        }

        layoutElement.Children = children;
        return layoutElement;
    }

    /// <summary>Wraps a container <c>Border</c> (see <see cref="ShimmerLayout.IsContainerProperty"/>)
    /// the same way a nested <see cref="Layout"/> is wrapped: captured for its own X/Y offset so
    /// descendants can still resolve their root-relative position, but never painted itself — only
    /// <paramref name="content"/> (and, if that's itself a <see cref="Layout"/>, everything under it) is.</summary>
    private static ShimmerLayoutElement ToShimmerContainerElement(this Border border, View content)
    {
        var containerElement = new ShimmerLayoutElement((float)border.X, (float)border.Y, (float)border.Width, (float)border.Height, border);

        ShimmerVisualElement child;
        if (content is Layout contentLayout)
        {
            child = contentLayout.ToShimmerLayoutElement();
        }
        else if (content is Border { Content: View nestedContent } nestedBorder && ShimmerLayout.GetIsContainer(nestedBorder))
        {
            child = nestedBorder.ToShimmerContainerElement(nestedContent);
        }
        else
        {
            child = content.ToShimmerVisualElement();
        }

        child.Parent = containerElement;
        containerElement.Children = [child];
        return containerElement;
    }

    /// <summary>Reduces a captured layout tree to just its drawable leaves — layouts themselves are
    /// never painted, only what's inside them.</summary>
    public static IEnumerable<ShimmerVisualElement> Flatten(this ShimmerLayoutElement layoutElement)
    {
        foreach (var child in layoutElement.Children)
        {
            if (child is ShimmerLayoutElement childLayout)
            {
                foreach (var descendant in childLayout.Flatten())
                    yield return descendant;
            }
            else
            {
                yield return child;
            }
        }
    }

    /// <summary>Root-relative X, computed by summing this element's local X with every ancestor's —
    /// <c>View.X</c> is only relative to its immediate parent, so this walk is what makes drawing
    /// leaves directly onto one shared canvas possible.</summary>
    public static float GetAbsoluteX(this ShimmerVisualElement element)
    {
        var x = element.X;
        for (var parent = element.Parent; parent != null; parent = parent.Parent)
            x += parent.X;

        return x;
    }

    /// <summary>Root-relative Y — see <see cref="GetAbsoluteX"/>.</summary>
    public static float GetAbsoluteY(this ShimmerVisualElement element)
    {
        var y = element.Y;
        for (var parent = element.Parent; parent != null; parent = parent.Parent)
            y += parent.Y;

        return y;
    }
}
