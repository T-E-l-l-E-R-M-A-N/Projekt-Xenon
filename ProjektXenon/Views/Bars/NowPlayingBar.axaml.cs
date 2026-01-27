using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProjektXenon.Helpers;
using ProjektXenon.Services;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class NowPlayingBar : UserControl
{
    private readonly UILayoutHelper _layoutHelper;

    public NowPlayingBar()
    {
        InitializeComponent();
        _layoutHelper = IoC.Resolve<UILayoutHelper>();
        IoC.Resolve<MainViewModel>().PropertyChanged += OnPropertyChanged;
        _layoutHelper.LayoutChanged += LayoutHelperOnLayoutChanged;
        _layoutHelper.ChangeLayout(_layoutHelper.IsMobileLayout);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentPage")
        {
            if (IoC.Resolve<MainViewModel>().CurrentPage.Type == PageType.NowPlaying)
            {
                IsVisible = false;
            }
            else
            {
                IsVisible = true;
            }
        }
    }

    private void LayoutHelperOnLayoutChanged(object? sender, bool e)
    {
        return;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_layoutHelper.IsMobileLayout)
        {
            MobileLayout.IsVisible = true;
            DesktopLayout.IsVisible = false;
            DesktopCompactLayout.IsVisible = false;
        }
        else
        {
            if (e.NewSize.Width < 880)
            {
                MobileLayout.IsVisible = false;
                DesktopLayout.IsVisible = false;
                DesktopCompactLayout.IsVisible = true;
            }
            else
            {
                MobileLayout.IsVisible = false;
                DesktopLayout.IsVisible = true;
                DesktopCompactLayout.IsVisible = false;
            }
        }
    }
}