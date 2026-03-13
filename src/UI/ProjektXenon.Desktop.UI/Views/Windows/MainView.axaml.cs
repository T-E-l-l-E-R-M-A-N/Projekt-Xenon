using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.Desktop.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }
}