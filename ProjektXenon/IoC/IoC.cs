using Microsoft.Extensions.DependencyInjection;
using ProjektXenon.ViewModels;

namespace ProjektXenon;

public static class IoC
{
    private static ServiceProvider _provider;

    public static void Build()
    {
        var services = new ServiceCollection();

        services.AddScoped<OsXAudioEngine>();
        services.AddScoped<MediaPlaybackService>();

        services.AddScoped<TrackRepositoryService>();

        services.AddScoped<AppBarViewModel>();
        services.AddScoped<NowPlayingBarViewModel>();
        services.AddScoped<NavMenuFlyoutViewModel>();
        services.AddScoped<PlaylistFlyoutViewModel>();
        services.AddScoped<NowPlayingFlyoutViewModel>();

        services.AddScoped<IPage, ExplorePageViewModel>();
        services.AddScoped<IPage, SearchPageViewModel>();
        services.AddScoped<IPage, FavoritesPageViewModel>();
        services.AddScoped<IPage, NowPlayingPageViewModel>();

        services.AddScoped<MainViewModel>();

        services.AddScoped<UILayoutHelper>();

        _provider = services.BuildServiceProvider();
    }

    public static T Resolve<T>() => _provider.GetRequiredService<T>();
}