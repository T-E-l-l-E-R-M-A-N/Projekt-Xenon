using ProjektXenon.Services;

namespace ProjektXenon.ViewModels;

public partial class NowPlayingPageViewModel : ViewModelBase, IPage
{
    #region Page Base

    public string Name => "Сейчас играет";
    public PageType Type => PageType.NowPlaying;

    #endregion
    
    #region Private Fields

    private readonly MediaPlaybackService _playbackService;
    private readonly TrackRepositoryService _trackRepository;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ICollectionItem? _currentMedia;
    [ObservableProperty] private PlaylistItem? _playlistItem;
    [ObservableProperty] private TimeSpan? _currentTime;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private double _totalTime;
    [ObservableProperty] private double _position;
    [ObservableProperty] private string? _totalTimeString;
    [ObservableProperty] private bool _playlistIsOpen;
    [ObservableProperty] private bool _isAvailable;

    #endregion

    #region Constructor

    public NowPlayingPageViewModel(MediaPlaybackService playbackService, TrackRepositoryService trackRepository)
    {
        _playbackService = playbackService;
        _trackRepository = trackRepository;
    }

    #endregion

    #region Commands Methods

    [RelayCommand]
    private void TogglePlaylistIsVisible()
    {
        PlaylistIsOpen = !PlaylistIsOpen;
    }

    private bool CanTogglePause() => CurrentMedia != null;

    [RelayCommand(CanExecute = "CanTogglePause")]
    private void TogglePause()
    {
        _playbackService.Pause();
        
        TogglePauseCommand.NotifyCanExecuteChanged();
    }
    private bool CanSkipNext()
    {
        if (_playbackService.Playlist != null)
        {
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((Models.MediaItem)CurrentMedia);
                if (index != _playbackService.Playlist.Media.Count - 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [RelayCommand(CanExecute = "CanSkipNext")]
    private void SkipNext()
    {
        if (_playbackService.Playlist != null)
        {
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((Models.MediaItem)CurrentMedia);
                if (index != _playbackService.Playlist.Media.Count - 1)
                {
                    index++;
                    var media = _playbackService.Playlist.Media[index];
                    _playbackService.OpenPlayAsync(media);
                }
            }
        }
    }
    
    private bool CanSkipPrevious()
    {
        if (_playbackService.Playlist != null)
        {
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((Models.MediaItem)CurrentMedia);
                if (index != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [RelayCommand(CanExecute = "CanSkipPrevious")]
    private void SkipPrevious()
    {
        var index = _playbackService.Playlist.Media.IndexOf((Models.MediaItem)CurrentMedia);
        if (index != 0)
        {
            index--;
            var media = _playbackService.Playlist.Media[index];
            _playbackService.OpenPlayAsync(media);
        }
    }

    [RelayCommand]
    private async Task ToggleFavorite()
    {
        (CurrentMedia as Models.MediaItem).IsFavorite = !(CurrentMedia as Models.MediaItem).IsFavorite;
        var favs = await _trackRepository.GetFavoritesAsync();
        List<Models.MediaItem> list = [];
        if (favs != null)
        {
            list.AddRange(favs);
        }
        
        if ((CurrentMedia as Models.MediaItem).IsFavorite)
        {
            if (list.FirstOrDefault(x => x.Id == CurrentMedia.Id) == null)
            {
                list.Add(CurrentMedia as Models.MediaItem);
            }
        }
        else
        {
            if (list.FirstOrDefault(x => x.Id == CurrentMedia.Id) is Models.MediaItem media)
            {
                list.Remove(media);
            }
        }

        
        await _trackRepository.ChangeFavoritesAsync(list);
    }

    [RelayCommand]
    private void Seek(object value)
    {
        if (value is double d)
            _playbackService.SeekTo(d);
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        _playbackService.TimeChanged += PlaybackServiceOnCurrentTimeChanged;
        //_playbackService.MediaPlayEnded += PlaybackServiceOnMediaPlayEnded;
        _playbackService.StateChanged += PlaybackServiceOnStateChanged;
        _playbackService.MediaChanged += PlaybackServiceOnMediaChanged;
    }

    #endregion

    #region Events Handlers

    private void PlaybackServiceOnMediaChanged(object? sender, Models.MediaItem e)
    {
        CurrentMedia = e;
        if (CurrentMedia != null)
        {
            IsAvailable = true;
        }
        else
        {
            IsAvailable = false;
            var navMenu = IoC.Resolve<NavMenuFlyoutViewModel>();
            navMenu.NavigateCommand.Execute(navMenu.Pages.OfType<ExplorePageViewModel>().FirstOrDefault());
        }
        if (_playbackService.Playlist != null) 
            PlaylistItem = _playbackService.Playlist;
    }
    
    private void PlaybackServiceOnStateChanged(object? sender, bool e)
    {
        IsPlaying = e;
        SkipPreviousCommand?.NotifyCanExecuteChanged();
        SkipNextCommand?.NotifyCanExecuteChanged();
        togglePauseCommand?.NotifyCanExecuteChanged();

        if (_playbackService.Playlist != null)
            foreach (var item in _playbackService.Playlist.Media)
            {
                (item as Models.MediaItem).IsPlaying = false;
                if (CurrentMedia != null && item.Id == CurrentMedia.Id)
                {
                    (item as Models.MediaItem).IsPlaying = true;
                }
            }
    }

    private void PlaybackServiceOnCurrentTimeChanged(object? sender, TimeSpan e)
    {
        CurrentTime = e;
        Position = CurrentTime.Value.TotalSeconds;
        if(CurrentMedia is MediaItem item)
            if (item.Time != null)
                TotalTime = item.Time.Value.TotalSeconds;
    }

    #endregion
}