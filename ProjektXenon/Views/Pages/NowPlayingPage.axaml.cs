using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProjektXenon.Helpers;
using ProjektXenon.Services;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class NowPlayingPage : UserControl
{
    private readonly UILayoutHelper _layoutHelper;

    public NowPlayingPage()
    {
        InitializeComponent();
        _layoutHelper = IoC.Resolve<UILayoutHelper>();
        _layoutHelper.LayoutChanged += LayoutHelperOnLayoutChanged;
        _layoutHelper.ChangeLayout(_layoutHelper.IsMobileLayout);
    }

    private void LayoutHelperOnLayoutChanged(object? sender, bool e)
    {
        if (_layoutHelper.IsMobileLayout)
        {
            MobileLayout.IsVisible = true;
            DesktopLayout.IsVisible = false;
        }
        else
        {
            MobileLayout.IsVisible = false;
            DesktopLayout.IsVisible = true;
        }
    }
    
    [RelayCommand]
    private void SwipeForward()
    {
        IoC.Resolve<NowPlayingBarViewModel>().SkipNextCommand.Execute(null);
    }

    [RelayCommand]
    private void SwipeBack()
    {
        
        IoC.Resolve<NowPlayingBarViewModel>().SkipPreviousCommand.Execute(null);
    }
}