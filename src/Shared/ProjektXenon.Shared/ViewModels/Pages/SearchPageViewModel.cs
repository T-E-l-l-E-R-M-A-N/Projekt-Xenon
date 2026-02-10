using System.Collections.ObjectModel;
using ListRandomizer;

namespace ProjektXenon.Shared.ViewModels;

public partial class SearchPageViewModel : ViewModelBase, IPage
{
    #region Page Base

    public string Name => "Поиск";
    public PageType Type => PageType.Search;

    #endregion

    #region Private Fields

    private readonly TrackRepositoryService _trackRepository;
    private readonly MediaPlaybackService _playbackService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ObservableCollection<Models.MediaItem>? _tracks;

    [ObservableProperty] private string? _searchText;
    [ObservableProperty] private bool _isAvailable;

    //[ObservableProperty] private ObservableCollection<PlaylistItem>? _savedPlaylists;

    #endregion

    #region Constructor

    public SearchPageViewModel(TrackRepositoryService trackRepository, MediaPlaybackService playbackService)
    {
        _trackRepository = trackRepository;
        _playbackService = playbackService;
        IsAvailable = true;
    }

    #endregion

    #region Public Methods

    

    #endregion

    #region Commands Methods

    [RelayCommand]
    private async Task Search()
    {
        var tracks = await _trackRepository.SearchAsync(SearchText);
        if (tracks != null)
            Tracks = new ObservableCollection<MediaItem>(tracks);
    }

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

        media.Shuffle();
        playlist.Media = new ObservableCollection<Models.MediaItem>(media);

        _playbackService.SetPlaylist(playlist);
        await _playbackService.OpenPlayAsync(playlist.Media[0]);
    }

    #endregion

}