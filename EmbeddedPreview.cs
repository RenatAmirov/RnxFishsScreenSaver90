using System.Diagnostics;
using System.Runtime.InteropServices;
using Aquarium90.Art;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Dispatching;

namespace Aquarium90;

// Windows supplies a Win32 parent from desk.cpl. Use a small native child with the
// same Win2D scene rather than reparenting a top-level WinUI window across processes.
internal sealed class EmbeddedPreview(nint parent, AquariumOptions options, Action ended)
{
    private CanvasDevice? device;
    private Aquarium? aquarium;
    private CanvasRenderTarget? buffer;
    private DispatcherQueueTimer? timer;
    private nint hwnd;
    private readonly Stopwatch clock = new();
    private double previous;
    private int childWidth, childHeight;

    internal void Start()
    {
        if (!NativeWindow.IsWindow(parent))
        {
            ended();
            return;
        }
        using (new NativeWindow.DpiScope(parent))
            hwnd = NativeWindow.CreateWindowEx(0, "STATIC", "", NativeWindow.PreviewSurface, 0, 0, 1, 1, parent, 0, 0, 0);
        if (hwnd == 0)
        {
            ended();
            return;
        }
        timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(40);
        timer.Tick += Render;
        clock.Start();
        timer.Start();
    }

    private void Render(DispatcherQueueTimer _, object args)
    {
        using var dpi = new NativeWindow.DpiScope(parent);
        if (!NativeWindow.GetClientRect(parent, out var rect) || !NativeWindow.IsWindow(hwnd))
        {
            Stop();
            return;
        }
        if (rect.Width <= 0 || rect.Height <= 0) return;
        if (rect.Width != childWidth || rect.Height != childHeight)
        {
            NativeWindow.SetWindowPos(hwnd, 0, 0, 0, rect.Width, rect.Height, NativeWindow.Show | NativeWindow.NoActivate);
            childWidth = rect.Width;
            childHeight = rect.Height;
            buffer?.Dispose();
            buffer = null;
        }

        try
        {
            device ??= CanvasDevice.GetSharedDevice();
            aquarium ??= new Aquarium(device, 90);
            // Limit GPU readback even if a larger-than-usual preview host is supplied.
            float scale = Math.Min(1f, 480f / Math.Max(childWidth, childHeight));
            int w = Math.Max(1, (int)(scale * childWidth)), h = Math.Max(1, (int)(scale * childHeight));
            buffer ??= new CanvasRenderTarget(device, w, h, 96);
            var now = clock.Elapsed.TotalSeconds;
            using (var draw = buffer.CreateDrawingSession())
            {
                draw.Clear(Ink.C(0x052A36));
                aquarium.Draw(draw, w, h, (float)Math.Clamp(now - previous, 0, .1), options);
            }
            previous = now;
            var info = new BitmapHeader
            {
                Size = 40, Width = w, Height = -h, Planes = 1, BitCount = 32
            };
            nint dc = NativeWindow.GetDC(hwnd);
            if (dc == 0) return;
            try
            {
                StretchDIBits(dc, 0, 0, childWidth, childHeight, 0, 0, w, h,
                    buffer.GetPixelBytes(), ref info, 0, 0x00CC0020); // SRCCOPY
            }
            finally { NativeWindow.ReleaseDC(hwnd, dc); }
        }
        catch (Exception e) when (device?.IsDeviceLost(e.HResult) == true)
        {
            buffer?.Dispose();
            aquarium?.Dispose();
            buffer = null;
            aquarium = null;
            device = null;
        }
    }

    private void Stop()
    {
        timer?.Stop();
        buffer?.Dispose();
        aquarium?.Dispose();
        if (NativeWindow.IsWindow(hwnd)) NativeWindow.DestroyWindow(hwnd);
        ended();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapHeader
    {
        internal uint Size;
        internal int Width, Height;
        internal ushort Planes, BitCount;
        internal uint Compression, SizeImage;
        internal int XPelsPerMeter, YPelsPerMeter;
        internal uint ClrUsed, ClrImportant;
    }

    [DllImport("gdi32.dll")]
    private static extern int StretchDIBits(nint dc, int x, int y, int width, int height,
        int srcX, int srcY, int srcWidth, int srcHeight, byte[] pixels, ref BitmapHeader info,
        uint usage, uint rasterOperation);
}
