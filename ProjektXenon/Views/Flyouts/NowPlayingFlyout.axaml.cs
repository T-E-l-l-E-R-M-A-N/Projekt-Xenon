using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjektXenon.Helpers;
using ProjektXenon.Services;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class NowPlayingFlyout : UserControl
{
    private UILayoutHelper _layoutHelper;

    public NowPlayingFlyout()
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
}