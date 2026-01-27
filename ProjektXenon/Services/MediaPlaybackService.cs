using System.Collections.ObjectModel;

namespace ProjektXenon.Services;

public class MediaPlaybackService   
{
    #region Private Fields

    private UniversalAudioEngine _audioEngine;

    #endregion

    #region Public Properties
    public PlaylistItem? Playlist { get; private set; }
    public Models.MediaItem? CurrentMedia { get; set; }
    public bool IsPlaying { get; private set; }
    public bool AutoPlay { get; set; }
    public TimeSpan Time { get; set; }
    #endregion

    #region Events
    public event EventHandler<TimeSpan>? TimeChanged;
    public event EventHandler? MediaPlayEnded;
    public event EventHandler<bool>? StateChanged;
    public event EventHandler<Models.MediaItem>? MediaChanged;
    public event EventHandler<PlaylistItem> PlaylistChsanged;
    #endregion

    #region Constructor

    public MediaPlaybackService(UniversalAudioEngine audioEngine)
    {
        _audioEngine = audioEngine;
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        _audioEngine.Init();
        _audioEngine.CurrentTimeChanged += AudioEngineOnCurrentTimeChanged;
        _audioEngine.MediaPlayEnded += AudioEngineOnMediaPlayEnded;
        _audioEngine.StateChanged += AudioEngineOnStateChanged;
    }

    public void SetPlaylist(PlaylistItem playlistItem)
    {
        Playlist = playlistItem;
        PlaylistChsanged?.Invoke(this, Playlist);
        AutoPlay = true;
    }

    public async Task OpenPlayAsync(Models.MediaItem media)
    {
        if (Playlist == null)
        {
            AutoPlay = false;
        }
        await _audioEngine.Open(media);
        _audioEngine.Play();
        CurrentMedia = media;
        if (CurrentMedia != null)
        {
            CurrentMedia.IsPlaying = true;
            MediaChanged?.Invoke(this, CurrentMedia);
        }
    }

    public async Task OpenPlayAsync(IEnumerable<MediaItem> media)
    {
        SetPlaylist(new PlaylistItem() {Media = new ObservableCollection<MediaItem>(media)});
        await OpenPlayAsync(Playlist.Media.FirstOrDefault());
    }
    
    public void Pause()
    {
        if(IsPlaying)
            _audioEngine.Pause();
        else
        {
            _audioEngine.Play();
        }
        
    }

    public void Stop()
    {
        _audioEngine.Stop();
    }

    public void SeekTo(double d)
    {
        _audioEngine.SeekTo(d);
    }

    #endregion

    #region Events Handlers
    private void AudioEngineOnStateChanged(object? sender, bool e)
    {
        IsPlaying = e;
        StateChanged?.Invoke(this, IsPlaying);
    }

    private async void AudioEngineOnMediaPlayEnded(object? sender, EventArgs e)
    {
        MediaPlayEnded?.Invoke(this, EventArgs.Empty);
        if (AutoPlay)
        {
            if (Playlist != null)
            {
                if (CurrentMedia != null)
                {
                    var index = Playlist.Media.IndexOf((Models.MediaItem)CurrentMedia);
                    if (index != Playlist.Media.Count - 1)
                    {
                        index++;
                        CurrentMedia = Playlist.Media[index];
                        await OpenPlayAsync(CurrentMedia as Models.MediaItem);
                    }
                }
            }
        }
    }

    private void AudioEngineOnCurrentTimeChanged(object? sender, TimeSpan e)
    {
        Time = e;
        TimeChanged?.Invoke(this, Time);
    }

    #endregion
}