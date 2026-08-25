namespace Plugin.Maui.Shimmer.Internals;

/// <summary>
/// Converts a CSS-style gradient <see cref="ShimmerLayout.Angle"/> (degrees, 0 = left-to-right,
/// increasing clockwise) into a start/end point pair suitable for
/// <see cref="Microsoft.Maui.Graphics.LinearGradientPaint"/>. Ported unchanged from the CSS Gradient
/// spec's own angle math — the two points aren't clamped to the unit square, since
/// <c>LinearGradientPaint</c> (like the CSS spec) is fine with stops outside [0,1].
/// <see cref="ClampNegligible"/> only snaps floating-point noise (values within <c>eps</c> of zero,
/// e.g. <c>cos(90°)</c> landing on <c>6.1e-17</c> instead of exactly <c>0</c>) — it must never zero
/// out a real negative coordinate, or <see cref="ToGradientPoints"/> stops being symmetric: <c>Start</c>
/// is always the point diametrically opposite <c>End</c> on the unit circle, so clamping away its
/// negative components collapses the gradient line to half its intended length instead of spanning
/// the full band corner-to-corner.
/// </summary>
internal static class GradientAngleExtensions
{
    public static (Point Start, Point End) ToGradientPoints(this double angle)
    {
        var d = Math.Pow(2, .5);
        var eps = Math.Pow(2, -52);

        var finalAngle = angle % 360;

        var startPointRadians = (180 - finalAngle).ToRadians();
        var startX = d * Math.Cos(startPointRadians);
        var startY = d * Math.Sin(startPointRadians);

        var endPointRadians = (360 - finalAngle).ToRadians();
        var endX = d * Math.Cos(endPointRadians);
        var endY = d * Math.Sin(endPointRadians);

        return (
            new Point(startX.ClampNegligible(eps), startY.ClampNegligible(eps)),
            new Point(endX.ClampNegligible(eps), endY.ClampNegligible(eps)));
    }

    private static double ToRadians(this double angle) => Math.PI * angle / 180;

    private static double ClampNegligible(this double value, double eps) =>
        Math.Abs(value) <= eps ? 0d : value;
}
