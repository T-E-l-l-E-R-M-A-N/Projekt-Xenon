using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.Desktop.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        CustomTitleBar.PointerPressed += MainView_PointerPressed  ;
    }

    private void MainView_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow.BeginMoveDrag(e);
    }
}