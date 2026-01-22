using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class AppBar : UserControl
{
    private readonly UILayoutHelper _layoutHelper;

    public AppBar()
    {
        InitializeComponent();
        IoC.Resolve<MainViewModel>().PropertyChanged += OnPropertyChanged;
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

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentPage")
        {
            if (IoC.Resolve<MainViewModel>().CurrentPage.Type == PageType.Search)
            {
                SearchBox.IsVisible = false;
                DesktopSearchButton.IsVisible = false;
            }
            else
            {
                SearchBox.IsVisible = true;
                DesktopSearchButton.IsVisible = true;
            }
        }
    }
}