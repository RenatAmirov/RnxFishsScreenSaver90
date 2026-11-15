using Aquarium90.Art;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Aquarium90;

internal sealed class SettingsWindow : Window
{
    internal SettingsWindow(AquariumOptions options, Action<AquariumOptions> runFullScreen)
    {
        Title = "Аквариум 90";
        var page = new SettingsPage(options, runFullScreen);
        Content = page;
        Closed += (_, _) => page.Dispose();

        if (AppWindowTitleBar.IsCustomizationSupported())
            CustomizeTitleBar();
        NativeWindow.SizeAndCenter(this, 1030, 668);
    }

    private void CustomizeTitleBar()
    {
        var titleBar = AppWindow.TitleBar;
        titleBar.BackgroundColor = Ink.C(0x091C24);
        titleBar.ForegroundColor = Ink.C(0xDDF4EC);
        titleBar.InactiveBackgroundColor = Ink.C(0x091C24);
        titleBar.InactiveForegroundColor = Ink.C(0x9AB5B0);
        titleBar.ButtonForegroundColor = Ink.C(0xDDF4EC);
        titleBar.ButtonBackgroundColor = Ink.C(0x091C24);
        titleBar.ButtonInactiveBackgroundColor = Ink.C(0x091C24);
        titleBar.ButtonInactiveForegroundColor = Ink.C(0x9AB5B0);
        titleBar.ButtonHoverBackgroundColor = Ink.C(0x1C3D45);
    }
}
