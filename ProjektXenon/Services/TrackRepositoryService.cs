using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;

namespace ProjektXenon.ViewModels;

public class TrackRepositoryService
{
    public event EventHandler FavoritesChanged;
    public async Task<IReadOnlyList<Models.MediaItem>?> GetFavoritesAsync()
    {
        if(File.Exists(Environment.CurrentDirectory + "/media/data.json"))
        {
            var json = File.OpenRead(Environment.CurrentDirectory + "/media/data.json");
            var list = await JsonSerializer.DeserializeAsync<List<Models.MediaItem>>(json);
            if (list != null) 
                return new ReadOnlyCollection<Models.MediaItem>(list);
        }

        return null;
    }

    public async Task<IReadOnlyList<PlaylistItem>?> GetPlaylistsAsync()
    {
        if(File.Exists(Environment.CurrentDirectory + "/media/playlists.json"))
        {
            var json = File.OpenRead(Environment.CurrentDirectory + "/media/playlists.json");
            var list = await JsonSerializer.DeserializeAsync<List<PlaylistItem>>(json);
            if (list != null) 
                return new ReadOnlyCollection<PlaylistItem>(list);
        }

        return null;
    }
    
    public async Task<IReadOnlyList<Models.MediaItem>?> GetTracksAsync()
    {
        
        var config = new Configuration().WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync("https://rus.hitmotop.com/songs/top-today");
        var trackslist = document.QuerySelector(".tracks__list");
        var tracks = trackslist.QuerySelectorAll(".tracks__item");
        var list = new List<Models.MediaItem>();
        foreach (var element in tracks)
        {
            var item = await CreateMediaItem(element);
            list.Add(item);
        }
        return list;
    }
    
    public async Task<IReadOnlyList<Models.MediaItem>?> SearchAsync(string text)
    {
        
        var config = new Configuration().WithDefaultLoader();
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync($"https://rus.hitmotop.com/search?q={text.Replace(" ", "%20")}");
        var trackslist = document.QuerySelector(".tracks__list");
        var tracks = trackslist.QuerySelectorAll(".tracks__item");
        var list = new List<Models.MediaItem>();
        foreach (var element in tracks)
        {
            var item = await CreateMediaItem(element);
            list.Add(item);
        }
        return list;
    }

    public async Task ChangeFavoritesAsync(IEnumerable<Models.MediaItem> media)
    {
        var serialized = JsonSerializer.Serialize(media, new JsonSerializerOptions() {WriteIndented = true, IndentSize = 4});
        await File.WriteAllTextAsync(Environment.CurrentDirectory + "/media/data.json", serialized);
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private async  Task<Models.MediaItem> CreateMediaItem(IElement trackItem)
    {
        var favs = await GetFavoritesAsync();
        
        var media = new Models.MediaItem();

        var musmeta = JsonSerializer.Deserialize<Dictionary<string, string>>(trackItem.GetAttribute("data-musmeta"));
        media.Name = musmeta["title"];
        media.Artist = musmeta["artist"];
        media.Url = musmeta["url"];
        media.Id = int.Parse(musmeta["id"].Replace("track-id-",""));
        media.IsFavorite = favs != null && favs.FirstOrDefault(x => x.Id == media.Id) != null;
        var cover = musmeta["img"];
        if (!cover.Contains("no-cover-150"))
            media.Image = cover;
        var time = "00:" + trackItem.QuerySelector(".track__fulltime").TextContent;
        media.Time = TimeSpan.Parse(time);
        
        return media;
    }
}

public class UILayoutHelper
{
    public bool IsMobileLayout { get; set; }

    public event EventHandler<bool> LayoutChanged;

    public void ChangeLayout(bool isMobile)
    {
        IsMobileLayout = isMobile;
        LayoutChanged?.Invoke(this, IsMobileLayout);
    }
}