using System.Collections.ObjectModel;
using ListRandomizer;
using ProjektXenon.Services;

namespace ProjektXenon.ViewModels;

public partial class ExplorePageViewModel : ViewModelBase, IPage
{
    #region Page Base

    public string Name => "Для вас";
    public PageType Type => PageType.Explore;

    #endregion

    #region Private Fields

    private readonly TrackRepositoryService _trackRepository;
    private readonly MediaPlaybackService _playbackService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ObservableCollection<Models.MediaItem>? _tracks;
    [ObservableProperty] private ObservableCollection<Models.MediaItem>? _favorites;
    [ObservableProperty] private bool _isAvailable;
    //[ObservableProperty] private ObservableCollection<PlaylistItem>? _savedPlaylists;

    #endregion

    #region Constructor

    public ExplorePageViewModel(TrackRepositoryService trackRepository, MediaPlaybackService playbackService)
    {
        _trackRepository = trackRepository;
        _playbackService = playbackService;
        IsAvailable = true;
    }

    #endregion

    #region Public Methods

    public async Task Init()
    {
        _trackRepository.FavoritesChanged += TrackRepositoryOnFavoritesChanged;
        var tracks = await _trackRepository.GetTracksAsync();
        if (tracks != null)
        {
            Tracks = new ObservableCollection<Models.MediaItem>(tracks);
        }

        var favorites = await _trackRepository.GetFavoritesAsync();
        if (favorites != null)
        {
            Favorites = new ObservableCollection<Models.MediaItem>(favorites);
        }
    }

    

    #endregion

    #region Commands Methods

    [RelayCommand]
    private async Task PlayAll()
    {
        var playlist = new PlaylistItem()
        {
            Id = Random.Shared.Next(),
            Name = "Current",
            Media = []
        };
        if (Tracks != null && Tracks.Any())
        {
            foreach (var track in Tracks)
                playlist.Media.Add((Models.MediaItem)track);
        }

        if (Favorites != null && Favorites.Any())
        {
            foreach (var track in Favorites)
                playlist.Media.Add((Models.MediaItem)track);
        }

        _playbackService.SetPlaylist(playlist);
        await _playbackService.OpenPlayAsync(playlist.Media[0]);
    }
    
    [RelayCommand]
    private async Task Shuffle()
    {
        var playlist = new PlaylistItem()
        {
            Id = Random.Shared.Next(),
            Name = "Current"
        };
        var media = new List<Models.MediaItem>();
        if (Tracks != null && Tracks.Any())
        {
            media.AddRange(Tracks.OfType<Models.MediaItem>());
        }

        if (Favorites != null && Favorites.Any())
        {
            media.AddRange(Favorites.OfType<Models.MediaItem>());
        }

        media.Shuffle();
        playlist.Media = new ObservableCollection<Models.MediaItem>(media);

        _playbackService.SetPlaylist(playlist);
        await _playbackService.OpenPlayAsync(playlist.Media[0]);
    }

    #endregion

    #region Events Handlers

    private void TrackRepositoryOnFavoritesChanged(object? sender, EventArgs e)
    {
        Init();
    }

    #endregion
}