namespace ProjektXenon.Shared.Models;

public partial class MediaItem : ViewModelBase, ICollectionItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Artist { get; set; }
    public string? Url { get; set; }
    public TimeSpan? Time { get; set; }
    public string? Image { get; set; }
    [ObservableProperty] private bool _isFavorite;
    [ObservableProperty] private bool _isPlaying;
}