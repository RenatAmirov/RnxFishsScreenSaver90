using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Aquarium90;

internal sealed class ScreenSaver(AquariumOptions options, Action ended)
{
    private readonly List<Window> windows = [];
    private readonly Stopwatch grace = new();
    private DispatcherQueueTimer? monitorInput;
    private NativeWindow.Point origin, previous;
    private uint lastInput;
    private int cursorHides;
    private bool stopping;

    internal void Start()
    {
        int seed = 90;
        foreach (var bounds in NativeWindow.MonitorBounds())
        {
            var view = new AquariumView(options, seed++);
            var window = new Window { Title = "Аквариум 90", Content = view };
            window.Closed += (_, _) =>
            {
                view.Dispose();
                windows.Remove(window);
                if (!stopping) Stop();
            };
            view.PointerPressed += (_, _) => { if (Ready) Stop(); };
            view.PointerWheelChanged += (_, _) => { if (Ready) Stop(); };
            view.KeyDown += (_, _) => { if (Ready) Stop(); };

            var appWindow = window.AppWindow;
            appWindow.IsShownInSwitchers = false;
            appWindow.MoveAndResize(new RectInt32(bounds.Left, bounds.Top, bounds.Width, bounds.Height));
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            windows.Add(window);
            window.Activate();
            NativeWindow.SetWindowPos(NativeWindow.Hwnd(window), NativeWindow.TopMost, bounds.Left, bounds.Top,
                bounds.Width, bounds.Height, NativeWindow.Show | NativeWindow.NoActivate);
        }
        if (windows.Count == 0) { Stop(); return; }

        int cursorCount;
        do { cursorCount = NativeWindow.ShowCursor(false); cursorHides++; }
        while (cursorCount >= 0);
        monitorInput = DispatcherQueue.GetForCurrentThread().CreateTimer();
        monitorInput.Interval = TimeSpan.FromMilliseconds(42);
        monitorInput.Tick += CheckInput;
        NativeWindow.GetCursorPos(out origin);
        previous = origin;
        lastInput = NativeWindow.LastInputTime();
        grace.Start();
        monitorInput.Start();
    }

    private bool Ready => grace.ElapsedMilliseconds >= 700;

    private void CheckInput(DispatcherQueueTimer _, object args)
    {
        NativeWindow.GetCursorPos(out var point);
        uint input = NativeWindow.LastInputTime();
        if (!Ready) origin = point;
        else
        {
            // Ignore a couple of pixels of sensor drift. A new input time with a stationary
            // pointer is a key/button/wheel event, including input outside the focused XAML element.
            if (Math.Abs(point.X - origin.X) > 4 || Math.Abs(point.Y - origin.Y) > 4
                || (lastInput != input && point.X == previous.X && point.Y == previous.Y))
            {
                Stop();
                return;
            }
        }
        previous = point;
        lastInput = input;
    }

    internal void Stop()
    {
        if (stopping) return;
        stopping = true;
        monitorInput?.Stop();
        while (cursorHides > 0) { NativeWindow.ShowCursor(true); cursorHides--; }
        foreach (var window in windows.ToArray()) window.Close();
        ended();
    }
}
