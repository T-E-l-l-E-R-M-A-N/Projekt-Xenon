using System.Collections.ObjectModel;

namespace ProjektXenon.Shared.Models;

public class PlaylistItem : ICollectionItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ObservableCollection<MediaItem> Media { get; set; }
}