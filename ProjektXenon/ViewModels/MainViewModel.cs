using System.IO;
using System.Net.Http;

namespace ProjektXenon.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Private Fields

    private readonly MediaPlaybackService _mediaPlaybackService;
    #endregion

    #region Observable Fields
    
    [ObservableProperty] private IPage? _currentPage;
    [ObservableProperty] private AppBarViewModel? _appBar;
    [ObservableProperty] private NowPlayingBarViewModel? _nowPlaying;
    [ObservableProperty] private NavMenuFlyoutViewModel? _navMenu;
    [ObservableProperty] private PlaylistFlyoutViewModel? _playlistView;
    [ObservableProperty] private NowPlayingFlyoutViewModel? _nowPlayingFlyout;

    #endregion

    #region Constructor

    public MainViewModel(MediaPlaybackService mediaPlaybackService)
    {
        _mediaPlaybackService = mediaPlaybackService;
    }

    #endregion

    #region Puiblic Methods

    public void Init()
    {
        _mediaPlaybackService.Init();
        
        AppBar = IoC.Resolve<AppBarViewModel>();
        NowPlaying = IoC.Resolve<NowPlayingBarViewModel>();
        NowPlaying.Init();
        NavMenu = IoC.Resolve<NavMenuFlyoutViewModel>();
        NavMenu.Init();
        PlaylistView = IoC.Resolve<PlaylistFlyoutViewModel>();
        NowPlayingFlyout = IoC.Resolve<NowPlayingFlyoutViewModel>();
        NowPlayingFlyout.Init();

        CurrentPage = NavMenu.Pages.OfType<ExplorePageViewModel>().FirstOrDefault();

        (CurrentPage as ExplorePageViewModel).Init();

        NavMenu.Pages.OfType<FavoritesPageViewModel>().FirstOrDefault().Init();
        NavMenu.Pages.OfType<NowPlayingPageViewModel>().FirstOrDefault().Init();
    }

    #endregion
    
    #region Commands Methods

    [RelayCommand]
    private async Task SearchAsync(string text)
    {
        if (string.IsNullOrEmpty(text)) 
        {
            CurrentPage = NavMenu.Pages.OfType<ExplorePageViewModel>().FirstOrDefault();
            return;
        }
        
        var search = NavMenu.Pages.OfType<SearchPageViewModel>().FirstOrDefault();
        search.SearchText = text;
        await search.SearchCommand.ExecuteAsync(null);
        CurrentPage = search;
    }

    [RelayCommand]
    private async Task PlayMedia(MediaItem media)
    {
        var path = Environment.CurrentDirectory + $"/media/{media.Id}.mp3";
        if (media.Url.StartsWith("https://") & !File.Exists(path))
        {
            using var hc = new HttpClient();
            var buffer = await hc.GetByteArrayAsync(media.Url);
            await File.WriteAllBytesAsync(path, buffer);
            media.Url = path;
            await _mediaPlaybackService.OpenPlayAsync(media);
            
        }
        else
        {
            media.Url = path;
            await _mediaPlaybackService.OpenPlayAsync(media);
        }
    }

    #endregion

    #region Events Handlers

    

    #endregion

}