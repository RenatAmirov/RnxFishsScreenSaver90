using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Aquarium90;

public partial class App : Application
{
    private Window? main;
    private ScreenSaver? saver;
    private EmbeddedPreview? preview;

    public App() => InitializeComponent();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var request = LaunchRequest.Parse(Environment.GetCommandLineArgs()[1..]);
        var options = AquariumOptions.Load();
        switch (request.Mode)
        {
            case LaunchMode.FullScreen:
                RunFullScreen(options);
                break;
            case LaunchMode.Embedded:
                preview = new EmbeddedPreview(request.Parent, options, Exit);
                preview.Start();
                break;
            case LaunchMode.Window:
                var view = new AquariumView(options, 90);
                var window = new Window { Title = "Аквариум 90", Content = view };
                main = window;
                view.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler((_, e) =>
                {
                    if (e.Key == VirtualKey.Escape) window.Close();
                }), true);
                window.Closed += (_, _) => { view.Dispose(); main = null; Exit(); };
                NativeWindow.SizeAndCenter(window, 1120, 690);
                window.Activate();
                break;
            case LaunchMode.Settings:
                var settings = new SettingsWindow(options, RunFullScreen);
                main = settings;
                settings.Closed += (_, _) =>
                {
                    main = null;
                    if (saver is null) Exit();
                    else saver.Stop();
                };
                if (NativeWindow.IsWindow(request.Parent))
                    NativeWindow.SetWindowLongPtr(NativeWindow.Hwnd(settings), -8, request.Parent); // GWLP_HWNDPARENT
                settings.Activate();
                break;
            default:
                Exit();
                break;
        }
    }

    private void RunFullScreen(AquariumOptions options)
    {
        if (saver is not null) return;
        main?.AppWindow.Hide();
        saver = new ScreenSaver(options, () =>
        {
            saver = null;
            if (main is null) Exit();
            else
            {
                main.AppWindow.Show();
                main.Activate();
            }
        });
        saver.Start();
    }
}
