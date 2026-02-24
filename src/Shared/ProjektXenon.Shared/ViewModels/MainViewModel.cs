namespace ProjektXenon.Shared.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Private Fields

    private readonly MediaPlaybackService _mediaPlaybackService;
    private readonly NavigationService _navigationService;
    #endregion

    #region Observable Fields
    
    [ObservableProperty] private IPage? _currentPage;
    [ObservableProperty] private AppBarViewModel? _appBar;
    [ObservableProperty] private NowPlayingBarViewModel? _nowPlaying;
    [ObservableProperty] private NavMenuFlyoutViewModel? _navMenu;
    [ObservableProperty] private PlaylistFlyoutViewModel? _playlistView;
    [ObservableProperty] private NowPlayingFlyoutViewModel? _nowPlayingFlyout;
    [ObservableProperty] private CommandBarViewModel? _commandBar;
    [ObservableProperty] private MediaContextMenuFlyoutViewModel? _mediaContextMenuFlyout;

    #endregion

    #region Constructor

    public MainViewModel(MediaPlaybackService mediaPlaybackService, NavigationService navigationService)
    {
        _mediaPlaybackService = mediaPlaybackService;
        _navigationService = navigationService;
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        _navigationService.Navigated += NavigationServiceOnNavigated;
        _mediaPlaybackService.Init();
        
        AppBar = IoC.Resolve<AppBarViewModel>();
        NowPlaying = IoC.Resolve<NowPlayingBarViewModel>();
        NowPlaying.Init();
        NavMenu = IoC.Resolve<NavMenuFlyoutViewModel>();
        NavMenu.Init();
        PlaylistView = IoC.Resolve<PlaylistFlyoutViewModel>();
        NowPlayingFlyout = IoC.Resolve<NowPlayingFlyoutViewModel>();
        NowPlayingFlyout.Init();
        CommandBar = IoC.Resolve<CommandBarViewModel>();
        CommandBar.Init();
        MediaContextMenuFlyout = IoC.Resolve<MediaContextMenuFlyoutViewModel>();

        _navigationService.NavigateToExplore();

        (CurrentPage as ExplorePageViewModel).Init();

        NavMenu.Pages.OfType<FavoritesPageViewModel>().FirstOrDefault().Init();
        NavMenu.Pages.OfType<NowPlayingPageViewModel>().FirstOrDefault().Init();
    }

    

    #endregion
    
    #region Commands Methods

    private bool CanReturn() => CurrentPage is not ExplorePageViewModel;
    [RelayCommand(CanExecute = "CanReturn")]
    private void Return()
    {
        _navigationService.NavigateToExplore();
    }

    [RelayCommand]
    private async Task SearchAsync(string text)
    {
        var search = NavMenu.Pages.OfType<SearchPageViewModel>().FirstOrDefault();
        if (!string.IsNullOrEmpty(text))
        {
            search.SearchText = text;
            await search.SearchCommand.ExecuteAsync(null);
        }
        _navigationService.NavigateToSearch();
    }

    [RelayCommand]
    private async Task PlayMedia(MediaItem media)
    {
        var path = TrackRepositoryService.AppDataPath + $"/{media.Id}.mp3";
        if (media.Url.StartsWith("https://") & !File.Exists(path))
        {
            using var hc = new HttpClient();
            var buffer = await hc.GetByteArrayAsync(media.Url);
            await File.WriteAllBytesAsync(path, buffer);
            media.Url = path;
            await _mediaPlaybackService.OpenPlayAsync(new [] {media});
            
        }
        else
        {
            media.Url = path;
            await _mediaPlaybackService.OpenPlayAsync(new [] {media});
        }
    }

    #endregion

    #region Events Handlers

    private void NavigationServiceOnNavigated(object? sender, IPage e)
    {
        CurrentPage = e;
        ReturnCommand?.NotifyCanExecuteChanged();
    }

    #endregion

}