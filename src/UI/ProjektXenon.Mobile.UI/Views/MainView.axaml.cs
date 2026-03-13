using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using ProjektXenon.Shared;
using ProjektXenon.Shared.ViewModels;

namespace ProjektXenon.Mobile.UI.Views;

public partial class MainView : UserControl
{
    public static readonly StyledProperty<ICommand> BackButtonCommandProperty = AvaloniaProperty.Register<MainView, ICommand>(
        nameof(BackButtonCommand));

    public ICommand BackButtonCommand
    {
        get => GetValue(BackButtonCommandProperty);
        set => SetValue(BackButtonCommandProperty, value);
    }
    public MainView()
    {
        IoC.Resolve<MainViewModel>().PropertyChanged += OnPropertyChanged;
        BackButtonCommand = new RelayCommand(async () =>
        {
            var dataContext = IoC.Resolve<MainViewModel>();
            PagePresenterView.Classes.Remove("IsOpen");
            await Task.Delay(500);
            dataContext.CurrentPage = null!;
        });
        InitializeComponent();
        PagePresenterView.Classes.Remove("IsOpen");
        IoC.Resolve<MainViewModel>().CurrentPage = null!;
        
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentPage")
        {
            if (IoC.Resolve<MainViewModel>().CurrentPage != null)
            {
                PagePresenterView.Classes.Add("IsOpen");
            }
        }
    }
}