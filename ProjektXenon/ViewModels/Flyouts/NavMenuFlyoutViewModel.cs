using System.Collections.ObjectModel;
using ProjektXenon.Services;

namespace ProjektXenon.ViewModels;

public partial class NavMenuFlyoutViewModel : ViewModelBase
{
    #region Private Fields

    private readonly NavigationService _navigationService;

    #endregion
    
    #region Observable Fields
    
    [ObservableProperty] private bool _isOpen;
    [ObservableProperty] private ObservableCollection<IPage> _pages;

    #endregion

    #region Constructor

    public NavMenuFlyoutViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        var pages = IoC.Resolve<IEnumerable<IPage>>();
        if (pages != null) Pages = new ObservableCollection<IPage>(pages);
    }

    #endregion

    #region Commands Methods
    [RelayCommand]
    private void CloseFlyout()
    {
        IsOpen = false;
    }

    [RelayCommand]
    private void Navigate(IPage p)
    {
        _navigationService.Navigate(p);
        CloseFlyout();
    }

    #endregion
}