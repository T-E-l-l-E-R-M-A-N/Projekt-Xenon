using System.Collections.ObjectModel;
using ListRandomizer;

namespace ProjektXenon.ViewModels;

public partial class FavoritesPageViewModel : ViewModelBase, IPage
{
    #region Page Base

    public string Name => "Нравится";
    public PageType Type => PageType.Favorites;

    #endregion

    #region Private Fields

    private readonly TrackRepositoryService _trackRepository;
    private readonly MediaPlaybackService _playbackService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ObservableCollection<Models.MediaItem>? _favorites;
    //[ObservableProperty] private ObservableCollection<PlaylistItem>? _savedPlaylists;

    #endregion

    #region Constructor

    public FavoritesPageViewModel(TrackRepositoryService trackRepository, MediaPlaybackService playbackService)
    {
        _trackRepository = trackRepository;
        _playbackService = playbackService;
    }

    #endregion

    #region Public Methods

    public async Task Init()
    {
        _trackRepository.FavoritesChanged += TrackRepositoryOnFavoritesChanged;
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