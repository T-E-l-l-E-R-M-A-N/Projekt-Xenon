using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class FavoritesPage : UserControl
{
    public FavoritesPage()
    {
        InitializeComponent();
        
        ChangeViewStyle();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == "DataContext")
        {
            if(DataContext is FavoritesPageViewModel)
                (DataContext as FavoritesPageViewModel).PropertyChanged += OnPropertyChanged;
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "View")
        {
            ChangeViewStyle();
        }
    }

    private void ChangeViewStyle()
    {
        var viewStyle = IoC.Resolve<NavMenuFlyoutViewModel>().Pages.OfType<FavoritesPageViewModel>().FirstOrDefault().View;
        switch (viewStyle)
        {
            case ListViewStyle:
                TileLayout.IsVisible = false;
                RowLayout.IsVisible = true;
                TableLayout.IsVisible = false;
                CarouselLayout.IsVisible = false;
                break;
            case TileViewStyle:
                TileLayout.IsVisible = true;
                RowLayout.IsVisible = false;
                TableLayout.IsVisible = false;
                CarouselLayout.IsVisible = false;
                break;
            case TableViewStyle:
                TileLayout.IsVisible = false;
                RowLayout.IsVisible = false;
                TableLayout.IsVisible = true;
                CarouselLayout.IsVisible = false;
                break;
            case CarouselViewStyle:
                TileLayout.IsVisible = false;
                RowLayout.IsVisible = false;
                TableLayout.IsVisible = false;
                CarouselLayout.IsVisible = true;
                break;
        }
    }

    private void DecreasePanoramaIndex(object? sender, RoutedEventArgs e)
    {
        PanoramaCarousel.SelectedIndex--;
    }

    private void IncreasePanoramaIndex(object? sender, RoutedEventArgs e)
    {
        PanoramaCarousel.SelectedIndex++;
    }

    [RelayCommand]
    private void MoveBack()
    {
        DecreasePanoramaIndex(PanoramaCarousel, new RoutedEventArgs());
    }

    [RelayCommand]
    private void MoveForward()
    {
        IncreasePanoramaIndex(PanoramaCarousel, new RoutedEventArgs());
    }
}