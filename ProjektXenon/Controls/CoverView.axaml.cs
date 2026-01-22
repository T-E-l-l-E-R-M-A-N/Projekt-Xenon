using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ProjektXenon.Controls;

public partial class CoverView : UserControl
{
    public static readonly StyledProperty<string> SourceProperty = AvaloniaProperty.Register<CoverView, string>(
        nameof(Source));

    public static readonly StyledProperty<CoverViewSize> SizeProperty = AvaloniaProperty.Register<CoverView, CoverViewSize>(
        nameof(Size), CoverViewSize.Small);

    public CoverViewSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string Source 
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    
    public CoverView()
    {
        InitializeComponent();
    }
}

public enum CoverViewSize
{
    Small,
    Medium,
    Large
}