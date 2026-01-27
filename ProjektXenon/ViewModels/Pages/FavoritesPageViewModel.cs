using System.Collections.ObjectModel;
using ListRandomizer;
using ProjektXenon.Services;

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
    private readonly NavigationService _navigationService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private ObservableCollection<Models.MediaItem>? _favorites;

    [ObservableProperty] private IViewStyle _view;
    [ObservableProperty] private ObservableCollection<IViewStyle> _views;
    [ObservableProperty] private bool _isAvailable;
    //[ObservableProperty] private ObservableCollection<PlaylistItem>? _savedPlaylists;

    #endregion

    #region Constructor

    public FavoritesPageViewModel(TrackRepositoryService trackRepository, MediaPlaybackService playbackService, NavigationService navigationService)
    {
        _trackRepository = trackRepository;
        _playbackService = playbackService;
        _navigationService = navigationService;
    }

    #endregion

    #region Public Methods

    public async Task Init()
    {
        Views = new ObservableCollection<IViewStyle>()
        {
            new TileViewStyle(),
            new ListViewStyle(),
            new TableViewStyle(),
            new CarouselViewStyle()
        };
        View = Views[0];
        
        _trackRepository.FavoritesChanged += TrackRepositoryOnFavoritesChanged;
        var favorites = await _trackRepository.GetFavoritesAsync();
        if (favorites != null)
        {
            Favorites = new ObservableCollection<Models.MediaItem>(favorites);
            IsAvailable = true;
        }
        else
        {
            IsAvailable = false;
            _navigationService.NavigateToExplore();
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

    [RelayCommand]
    private void SetView(IViewStyle? view)
    {
        if(view == null)
        {
            var index = Views.IndexOf(View);
            if (index < 3)
            {
                index++;
            }
            else
            {
                index = 0;
            }

            View = Views[index];
        }
        else
        {
            View = view;
        }
    }

    #endregion

    #region Events Handlers

    private async void TrackRepositoryOnFavoritesChanged(object? sender, EventArgs e)
    {
        await Init();
    }

    #endregion
}