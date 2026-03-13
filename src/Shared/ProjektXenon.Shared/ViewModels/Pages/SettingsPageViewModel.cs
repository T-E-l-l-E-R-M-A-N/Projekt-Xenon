using System.Collections.ObjectModel;

namespace ProjektXenon.Shared.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase, IPage
{
    #region Page Base

    public string Name => Strings.SettingsPageTitle;
    public PageType Type => PageType.Settings;

    #endregion

    #region Private Fields

    private readonly SettingsManagerService _settingsManagerService;

    #endregion
    
    #region Observable Fields

    [ObservableProperty] private bool _isAvailable = true;
    [ObservableProperty] private ObservableCollection<string> _themes;
    [ObservableProperty] private ObservableCollection<string> _languages;
    #endregion

    #region Public Properties

    public string Language
    {
        get => _settingsManagerService.Settings.Language;
        set => _settingsManagerService.SetLanguage(value);
    }

    public string Theme
    {
        get => _settingsManagerService.Settings.Theme;
        set => _settingsManagerService.SetTheme(value);
    }

    #endregion

    #region Constructor

    public SettingsPageViewModel(SettingsManagerService settingsManagerService)
    {
        _settingsManagerService = settingsManagerService;
        Init();
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        IoC.Resolve<TrackRepositoryService>().FavoritesChanged += OnFavoritesChanged;
        
        Languages = new ObservableCollection<string>()
        {
            "English",
            "Russian"
        };
        Themes = new ObservableCollection<string>()
        {
            "Default",
            "Light",
            "Dark"
        };
    }
    
    private void OnFavoritesChanged(object? sender, EventArgs e)
    {
        ClearAllFavoritesCommand?.NotifyCanExecuteChanged();
    }

    #endregion

    #region Commands Methods

    [RelayCommand] private void SetLightTheme() => Theme = "Light";
    [RelayCommand] private void SetDarkTheme() => Theme = "Dark";
    [RelayCommand] private void SetDefaultTheme() => Theme = "Default";

    private bool CanClearFavorites()
    {
        var favs = IoC.Resolve<MainViewModel>().Favorites;
        return favs != null && favs.Count > 0;
    }
    [RelayCommand(CanExecute = nameof(CanClearFavorites))]
    private async Task ClearAllFavorites()
    {
        var trackRep = IoC.Resolve<TrackRepositoryService>();
        await trackRep.ChangeFavoritesAsync([]);
    }

    #endregion
}