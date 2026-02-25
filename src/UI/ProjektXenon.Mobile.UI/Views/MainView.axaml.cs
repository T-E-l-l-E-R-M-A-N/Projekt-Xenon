using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProjektXenon.Shared;
using ProjektXenon.Shared.ViewModels;

namespace ProjektXenon.Mobile.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        IoC.Resolve<MainViewModel>().CurrentPage = null;
    }

    private void OnNavMenuItemClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dataContext = (sender as Avalonia.Controls.Button).DataContext as IPage;

        IoC.Resolve<MainViewModel>().CurrentPage = dataContext;
    }

    private void BackButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IoC.Resolve<MainViewModel>().CurrentPage = null;
    }
}