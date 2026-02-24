namespace ProjektXenon.Shared.ViewModels;

public partial class NowPlayingBarViewModel : ViewModelBase
{
    #region Constructor

    public NowPlayingBarViewModel(MediaPlaybackService playbackService, TrackRepositoryService trackRepository,
        NavigationService navigationService)
    {
        _playbackService = playbackService;
        _trackRepository = trackRepository;
        _navigationService = navigationService;
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

    #region Private Fields

    private readonly MediaPlaybackService _playbackService;
    private readonly TrackRepositoryService _trackRepository;
    private readonly NavigationService _navigationService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ICollectionItem? _currentMedia;
    [ObservableProperty] private PlaylistItem? _playlistItem;
    [ObservableProperty] private TimeSpan? _currentTime;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private double _totalTime;
    [ObservableProperty] private double _position;

    #endregion

    #region Commands Methods

    [RelayCommand]
    private void ToggleNowPlayingView()
    {
        _navigationService.NavigateToNowPlaying();
    }

    private bool CanTogglePause()
    {
        return CurrentMedia != null;
    }

    [RelayCommand(CanExecute = "CanTogglePause")]
    private void TogglePause()
    {
        _playbackService.Pause();

        TogglePauseCommand.NotifyCanExecuteChanged();
    }

    private bool CanSkipNext()
    {
        if (_playbackService.Playlist != null)
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((MediaItem)CurrentMedia);
                if (index != _playbackService.Playlist.Media.Count - 1) return true;
            }

        return false;
    }

    [RelayCommand(CanExecute = "CanSkipNext")]
    private void SkipNext()
    {
        if (_playbackService.Playlist != null)
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((MediaItem)CurrentMedia);
                if (index != _playbackService.Playlist.Media.Count - 1)
                {
                    index++;
                    var media = _playbackService.Playlist.Media[index];
                    _playbackService.OpenPlayAsync(media);
                }
            }
    }

    private bool CanSkipPrevious()
    {
        if (_playbackService.Playlist != null)
            if (CurrentMedia != null)
            {
                var index = _playbackService.Playlist.Media.IndexOf((MediaItem)CurrentMedia);
                if (index != 0) return true;
            }

        return false;
    }

    [RelayCommand(CanExecute = "CanSkipPrevious")]
    private void SkipPrevious()
    {
        var index = _playbackService.Playlist.Media.IndexOf((MediaItem)CurrentMedia);
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
        (CurrentMedia as MediaItem).IsFavorite = !(CurrentMedia as MediaItem).IsFavorite;
        var favs = await _trackRepository.GetFavoritesAsync();
        List<MediaItem> list = [];
        if (favs != null) list.AddRange(favs);

        if ((CurrentMedia as MediaItem).IsFavorite)
        {
            if (list.FirstOrDefault(x => x.Id == CurrentMedia.Id) == null) list.Add(CurrentMedia as MediaItem);
        }
        else
        {
            if (list.FirstOrDefault(x => x.Id == CurrentMedia.Id) is MediaItem media) list.Remove(media);
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

    #region Events Handlers

    private void PlaybackServiceOnMediaChanged(object? sender, MediaItem e)
    {
        CurrentMedia = e;
        if (_playbackService.Playlist != null)
            PlaylistItem = _playbackService.Playlist;
    }

    private void PlaybackServiceOnStateChanged(object? sender, bool e)
    {
        IsPlaying = e;
        SkipPreviousCommand.NotifyCanExecuteChanged();
        SkipNextCommand.NotifyCanExecuteChanged();
        TogglePauseCommand.NotifyCanExecuteChanged();

        if (_playbackService.Playlist != null)
            foreach (var item in _playbackService.Playlist.Media)
            {
                item.IsPlaying = false;
                if (CurrentMedia != null && item.Id == CurrentMedia.Id) item.IsPlaying = true;
            }
    }

    private void PlaybackServiceOnCurrentTimeChanged(object? sender, TimeSpan e)
    {
        CurrentTime = e;
        Position = CurrentTime.Value.TotalSeconds;
        if ((CurrentMedia as MediaItem) is { } item)
            TotalTime = item.Time.Value.TotalSeconds;
    }

    #endregion
}