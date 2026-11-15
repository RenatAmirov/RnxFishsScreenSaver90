using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI;

namespace Aquarium90.Art;

internal static class Ink
{
    internal static Color C(uint rgb, byte a = 255) =>
        Color.FromArgb(a, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    internal static Color Mix(Color x, Color y, float t) => Color.FromArgb(
        (byte)(x.A + (y.A - x.A) * t), (byte)(x.R + (y.R - x.R) * t),
        (byte)(x.G + (y.G - x.G) * t), (byte)(x.B + (y.B - x.B) * t));

    internal static Vector2 V(float x, float y) => new(x, y);
    internal static float Between(this Random random, float min, float max) =>
        min + (max - min) * random.NextSingle();

    // The first pair is the starting point; each subsequent six values are a cubic Bézier.
    internal static CanvasGeometry Curve(ICanvasResourceCreator device, bool closed,
        params ReadOnlySpan<float> points)
    {
        using var path = new CanvasPathBuilder(device);
        path.BeginFigure(points[0], points[1]);
        for (int i = 2; i < points.Length; i += 6)
            path.AddCubicBezier(V(points[i], points[i + 1]), V(points[i + 2], points[i + 3]),
                V(points[i + 4], points[i + 5]));
        path.EndFigure(closed ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);
        return CanvasGeometry.CreatePath(path);
    }

    internal static CanvasLinearGradientBrush Linear(ICanvasResourceCreator device,
        Vector2 start, Vector2 end, params CanvasGradientStop[] stops) =>
        new(device, stops) { StartPoint = start, EndPoint = end };

    internal static CanvasGradientStop Stop(float t, Color color) =>
        new() { Position = t, Color = color };

    internal static void Stroke(CanvasDrawingSession g, Color color, float width,
        params ReadOnlySpan<float> points)
    {
        using var path = Curve(g, false, points);
        g.DrawGeometry(path, color, width);
    }

    internal static void Fill(CanvasDrawingSession g, Color color, params ReadOnlySpan<float> points)
    {
        using var path = Curve(g, true, points);
        g.FillGeometry(path, color);
    }
}
