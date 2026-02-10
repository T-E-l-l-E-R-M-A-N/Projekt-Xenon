using System.ComponentModel;
using System.Timers;
using ManagedBass;
using Timer = System.Timers.Timer;

namespace ProjektXenon.Services;

public class UniversalAudioEngine
{
    #region Private Fields

    private MediaPlayer? _player;

    private Timer _timer;

    #endregion

    #region Events

    public event EventHandler<TimeSpan>? CurrentTimeChanged;
    public event EventHandler? MediaPlayEnded;
    public event EventHandler<bool>? StateChanged;

    #endregion

    #region Public Methods

    public void Init()
    {
        _player = new MediaPlayer();
        _player.PropertyChanged += PlayerOnPropertyChanged;
        _player.MediaEnded += PlayerOnMediaEnded;
        _timer = new Timer(1000);
        _timer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        CurrentTimeChanged?.Invoke(this, _player.Position);
    }

    public async Task Open(MediaItem media)
    {
        if (_player != null)
        {
            try
            {
                await _player.LoadAsync(media.Url);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }

    private void OnPlayerOnMediaLoaded(int i)
    {
        _player?.MediaLoaded -= OnPlayerOnMediaLoaded;
        Play();
    }

    public void Play()
    {
        _player?.Play();
        _timer.Start();
    }

    public void Pause()
    {
        _player?.Pause();
        _timer.Stop();
    }

    public void Stop()
    {
        _player?.Stop();
        _player?.Dispose();
        _timer.Stop();
    }

    public void SeekTo(double d)
    {
        _player.Position = TimeSpan.FromSeconds(d);
    }

    #endregion

    #region Events Handlers

    private void PlayerOnMediaEnded(object? sender, EventArgs e)
    {
        MediaPlayEnded?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "State")
        {
            StateChanged?.Invoke(this, _player != null && _player.State == PlaybackState.Playing);
        }
    }

    #endregion
}