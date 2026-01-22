using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class ExplorePage : UserControl
{
    private readonly UILayoutHelper _layoutHelper;

    public ExplorePage()
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