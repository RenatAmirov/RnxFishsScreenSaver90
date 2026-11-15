using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Aquarium90;

internal static class NativeWindow
{
    internal static nint Hwnd(Window window) => WinRT.Interop.WindowNative.GetWindowHandle(window);

    internal static void SizeAndCenter(Window window, int width, int height)
    {
        var screen = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var scale = GetDpiForWindow(Hwnd(window)) / 96f;
        int w = Math.Min((int)(width * scale), screen.Width - 24);
        int h = Math.Min((int)(height * scale), screen.Height - 24);
        window.AppWindow.MoveAndResize(new RectInt32(screen.X + (screen.Width - w) / 2,
            screen.Y + (screen.Height - h) / 2, w, h));
    }

    internal const uint ChildVisible = 0x50000000;
    internal const uint PreviewSurface = ChildVisible | 0x00000004; // SS_BLACKRECT
    internal const uint Show = 0x0040;
    internal const uint NoActivate = 0x0010;
    internal static readonly nint TopMost = new(-1);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        internal int Left, Top, Right, Bottom;
        internal readonly int Width => Right - Left;
        internal readonly int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Point { internal int X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInput { internal uint Size, Time; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        internal uint Size;
        internal Rect Monitor, Work;
        internal uint Flags;
    }

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool MonitorCallback(nint monitor, nint dc, nint rect, nint data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(nint dc, nint clip, MonitorCallback callback, nint data);
    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hwnd);
    [DllImport("user32.dll")]
    private static extern nint GetWindowDpiAwarenessContext(nint hwnd);
    [DllImport("user32.dll")]
    private static extern nint SetThreadDpiAwarenessContext(nint context);
    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInput info);
    [DllImport("user32.dll")]
    internal static extern bool GetCursorPos(out Point point);
    [DllImport("user32.dll")]
    internal static extern int ShowCursor(bool show);
    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(nint hwnd, nint after, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    internal static extern nint SetWindowLongPtr(nint hwnd, int index, nint value);
    [DllImport("user32.dll")]
    internal static extern bool IsWindow(nint hwnd);
    [DllImport("user32.dll")]
    internal static extern bool GetClientRect(nint hwnd, out Rect rect);
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
    internal static extern nint CreateWindowEx(uint exStyle, string className, string title,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll")]
    internal static extern bool DestroyWindow(nint hwnd);
    [DllImport("user32.dll")]
    internal static extern nint GetDC(nint hwnd);
    [DllImport("user32.dll")]
    internal static extern int ReleaseDC(nint hwnd, nint dc);

    internal static uint LastInputTime()
    {
        var info = new LastInput { Size = (uint)Marshal.SizeOf<LastInput>() };
        return GetLastInputInfo(ref info) ? info.Time : 0;
    }

    internal readonly struct DpiScope : IDisposable
    {
        private readonly nint previous;

        internal DpiScope(nint hwnd)
        {
            nint context = GetWindowDpiAwarenessContext(hwnd);
            previous = context == 0 ? 0 : SetThreadDpiAwarenessContext(context);
        }

        public void Dispose()
        {
            if (previous != 0) SetThreadDpiAwarenessContext(previous);
        }
    }

    internal static IReadOnlyList<Rect> MonitorBounds()
    {
        var monitors = new List<Rect>();
        EnumDisplayMonitors(0, 0, (monitor, _, _, _) =>
        {
            var info = new MonitorInfo { Size = (uint)Marshal.SizeOf<MonitorInfo>() };
            if (GetMonitorInfo(monitor, ref info))
            {
                if ((info.Flags & 1) == 0) monitors.Insert(0, info.Monitor);
                else monitors.Add(info.Monitor);
            }
            return true;
        }, 0);
        return monitors;
    }
}
