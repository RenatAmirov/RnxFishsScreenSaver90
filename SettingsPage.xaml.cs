using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Aquarium90;

public sealed partial class SettingsPage : UserControl, IDisposable
{
    private readonly Action<AquariumOptions> launch;
    private readonly AquariumView view;
    private AquariumOptions saved;
    private bool ready;

    internal SettingsPage(AquariumOptions options, Action<AquariumOptions> launch)
    {
        saved = options;
        this.launch = launch;
        InitializeComponent();
        view = new AquariumView(options, 90, 40);
        PreviewHost.Children.Add(view);
        Fish.Value = options.FishCount;
        Speed.Value = options.Speed;
        Bubbles.IsOn = options.Bubbles;
        ready = true;
        UpdateOptions();
    }

    private void SliderChanged(object sender, RangeBaseValueChangedEventArgs e) => UpdateOptions();
    private void BubblesChanged(object sender, RoutedEventArgs e) => UpdateOptions();

    private void UpdateOptions()
    {
        if (!ready) return;
        view.Options = new((int)Fish.Value, (int)Speed.Value, Bubbles.IsOn);
        FishValue.Text = view.Options.FishCount.ToString();
        SpeedValue.Text = $"{view.Options.Speed}%";
        Status.Text = view.Options == saved ? "Настройки сохранены" : "Есть несохранённые изменения";
    }

    private void LayoutChanged(object sender, SizeChangedEventArgs e)
    {
        if (OptionsPanel is null) return;
        bool narrow = e.NewSize.Width < 752;
        SettingsColumn.Width = narrow ? new GridLength(0) : new GridLength(292);
        Panels.ColumnSpacing = narrow ? 0 : 20;
        Panels.RowSpacing = narrow ? 20 : 0;
        Grid.SetColumn(OptionsPanel, narrow ? 0 : 1);
        Grid.SetRow(OptionsPanel, narrow ? 1 : 0);
        PreviewHost.Height = narrow ? Math.Clamp((e.NewSize.Width - 64) * .57, 160, 345) : 345;
    }

    private void FullScreenClicked(object sender, RoutedEventArgs e) => launch(view.Options);

    private void SaveClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            view.Options.Save();
            saved = view.Options;
            Status.Text = "Настройки сохранены";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Status.Text = "Не удалось сохранить настройки.";
        }
    }

    public void Dispose() => view.Dispose();
}
