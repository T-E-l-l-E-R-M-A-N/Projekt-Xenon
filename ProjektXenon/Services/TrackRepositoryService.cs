using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using AngleSharp;
using AngleSharp.Dom;

namespace ProjektXenon.Services;

public class TrackRepositoryService
{
    public event EventHandler FavoritesChanged;

    public async Task<IReadOnlyList<Models.MediaItem>?> GetFavoritesAsync()
    {
        if (File.Exists(App.AppDataPath + "/data.json"))
        {
            await using var json = File.OpenRead(App.AppDataPath + "/data.json");
            var list = await JsonSerializer.DeserializeAsync<List<Models.MediaItem>>(json);
            if (list != null)
                return new ReadOnlyCollection<Models.MediaItem>(list);
        }

        return null;
    }

    public async Task<IReadOnlyList<PlaylistItem>?> GetPlaylistsAsync()
    {
        if (File.Exists(App.AppDataPath + "/playlists.json"))
        {
            var json = File.OpenRead(App.AppDataPath + "/playlists.json");
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
        var document = await context.OpenAsync("https://rus.hitmotop.com/songs/top-rated");
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
        if (trackslist != null)
        {
            var tracks = trackslist.QuerySelectorAll(".tracks__item");
            var list = new List<Models.MediaItem>();
            foreach (var element in tracks)
            {
                var item = await CreateMediaItem(element);
                list.Add(item);
            }

            return list;
        }

        return [];
    }

    public async Task ChangeFavoritesAsync(IEnumerable<Models.MediaItem> media)
    {
        var serialized =
            JsonSerializer.Serialize(media, new JsonSerializerOptions() { WriteIndented = true, IndentSize = 4 });
        await File.WriteAllTextAsync(App.AppDataPath + "/data.json", serialized);
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<Models.MediaItem> CreateMediaItem(IElement trackItem)
    {
        var favs = await GetFavoritesAsync();

        var media = new Models.MediaItem();

        var musmeta = JsonSerializer.Deserialize<Dictionary<string, string>>(trackItem.GetAttribute("data-musmeta"));
        media.Name = musmeta["title"];
        media.Artist = musmeta["artist"];
        media.Url = musmeta["url"];
        media.Id = int.Parse(musmeta["id"].Replace("track-id-", ""));
        media.IsFavorite = favs != null && favs.FirstOrDefault(x => x.Id == media.Id) != null;
        var cover = musmeta["img"];
        if (!cover.Contains("no-cover-150"))
            media.Image = cover;
        var time = "00:" + trackItem.QuerySelector(".track__fulltime").TextContent;
        try
        {
            media.Time = TimeSpan.Parse(time);
        }
        catch
        {
            media.Time = TimeSpan.Parse("00:00:00");
        }

        return media;
    }

    private async Task<string?> TryGetImageAsync(string name, string artist)
    {
        var config = new Configuration().WithDefaultLoader();
        var context = BrowsingContext.New(config);
        try
        {
            var document =
                await context.OpenAsync(
                    $"https://www.discogs.com/ru/search?q={name.Replace(" ", "+")}+{artist.Replace(" ", "+")}");
            Thread.Sleep(5000);
            var firstListItem = document.QuerySelectorAll(".w-full text-black").FirstOrDefault();
            var imageItem = firstListItem.QuerySelector("img");
            return imageItem.GetAttribute("src");
        }
        catch (Exception e)
        {
            return null;
        }
    }
}