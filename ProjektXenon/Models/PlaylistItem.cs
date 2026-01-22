using System.Collections.ObjectModel;

namespace ProjektXenon.Models;

public class PlaylistItem : ICollectionItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ObservableCollection<Models.MediaItem> Media { get; set; }
}