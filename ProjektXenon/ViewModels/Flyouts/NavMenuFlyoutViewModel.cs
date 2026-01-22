using System.Collections.ObjectModel;

namespace ProjektXenon.ViewModels;

public partial class NavMenuFlyoutViewModel : ViewModelBase
{
    #region Private Fields

    private readonly MainViewModel _mainViewModel;

    #endregion
    
    #region Observable Fields
    
    [ObservableProperty] private bool _isOpen;
    [ObservableProperty] private ObservableCollection<IPage> _pages;

    #endregion

    #region Constructor

    public NavMenuFlyoutViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
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
        _mainViewModel.CurrentPage = p;
        CloseFlyout();
    }

    #endregion
}