using System.Diagnostics;
using Aquarium90.Art;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Aquarium90;

internal sealed class AquariumView : Grid, IDisposable
{
    private readonly CanvasControl canvas;
    private readonly DispatcherQueueTimer ticker;
    private readonly Stopwatch clock = new();
    private Aquarium? aquarium;
    private double previous;
    private bool disposed;
    internal AquariumOptions Options { get; set; }

    internal AquariumView(AquariumOptions options, int seed, int fps = 60)
    {
        Options = options;
        Background = new SolidColorBrush(Ink.C(0x05242D));
        canvas = new CanvasControl();
        canvas.CreateResources += (sender, _) =>
        {
            aquarium?.Dispose();
            aquarium = new Aquarium(sender.Device, seed);
        };
        canvas.Draw += (sender, e) =>
        {
            double now = clock.Elapsed.TotalSeconds;
            float elapsed = (float)Math.Clamp(now - previous, 0, .10);
            previous = now;
            aquarium?.Draw(e.DrawingSession, (float)sender.ActualWidth, (float)sender.ActualHeight, elapsed, Options);
        };
        Children.Add(canvas);
        ticker = DispatcherQueue.CreateTimer();
        ticker.Interval = TimeSpan.FromSeconds(1.0 / fps);
        ticker.Tick += (_, _) => canvas.Invalidate();
        Loaded += (_, _) => { if (!disposed) { clock.Start(); ticker.Start(); } };
        Unloaded += (_, _) => { clock.Stop(); ticker.Stop(); };
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ticker.Stop();
        aquarium?.Dispose();
        aquarium = null;
        canvas.RemoveFromVisualTree();
    }
}
