using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.Controls;

public partial class TrackLabel : UserControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<TrackLabel, string>(
        nameof(Title));

    public static readonly StyledProperty<string> SubtitleProperty = AvaloniaProperty.Register<TrackLabel, string>(
        nameof(Subtitle));

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    
    public TrackLabel()
    {
        InitializeComponent();
    }
}