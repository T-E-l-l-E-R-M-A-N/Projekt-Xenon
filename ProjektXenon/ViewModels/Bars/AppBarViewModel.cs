namespace ProjektXenon.ViewModels;

public partial class AppBarViewModel : ViewModelBase
{
    #region Private Fields

    private readonly NavMenuFlyoutViewModel? _navMenu;

    #endregion

    #region Contstructor

    public AppBarViewModel(NavMenuFlyoutViewModel? navMenu)
    {
        _navMenu = navMenu;
    }

    #endregion
    
    #region Commands Methods

    [RelayCommand]
    private void ToggleNavMenu()
    {
        _navMenu.IsOpen = !_navMenu.IsOpen;
    }

    #endregion
}