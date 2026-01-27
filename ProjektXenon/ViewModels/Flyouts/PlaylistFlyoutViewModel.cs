using ProjektXenon.Services;

namespace ProjektXenon.ViewModels;

public partial class PlaylistFlyoutViewModel : ViewModelBase
{
    #region Private Fields

    private readonly MediaPlaybackService _playbackService;

    #endregion

    #region Observable Fields

    [ObservableProperty] private bool _isOpen;
    [ObservableProperty] private PlaylistItem? _playlist;

    #endregion

    #region Constructor

    public PlaylistFlyoutViewModel(MediaPlaybackService playbackService)
    {
        _playbackService = playbackService;
        Init();
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        _playbackService.PlaylistChsanged += PlaybackServiceOnPlaylistChsanged;
    }

    #endregion

    #region Events Handlers

    private void PlaybackServiceOnPlaylistChsanged(object? sender, PlaylistItem e)
    {
        Playlist = e;
    }

    #endregion
}