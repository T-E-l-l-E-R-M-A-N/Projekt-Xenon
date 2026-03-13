using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Window = Avalonia.Controls.Window;

namespace ProjektXenon.Desktop.UI.Views;

public partial class FullScreenWindow : Window
{
    public FullScreenWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}