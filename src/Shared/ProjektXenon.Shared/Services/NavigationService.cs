using ProjektXenon.Shared;

namespace ProjektXenon.Services;

public class NavigationService
{
    public event EventHandler<IPage>? Navigated;
    
    public IPage? CurrentPage { get; private set; }

    public void Navigate(IPage p)
    {
        CurrentPage = p;
        Navigated?.Invoke(this, CurrentPage);
    }

    public void NavigateToExplore()
    {
        var page = IoC.Resolve<IEnumerable<IPage>>().OfType<ExplorePageViewModel>().FirstOrDefault();
        Navigate(page);
    }

    public void NavigateToSearch()
    {
        var page = IoC.Resolve<IEnumerable<IPage>>().OfType<SearchPageViewModel>().FirstOrDefault();
        Navigate(page);
    }

    public void NavigateToNowPlaying()
    {
        var page = IoC.Resolve<IEnumerable<IPage>>().OfType<NowPlayingPageViewModel>().FirstOrDefault();
        Navigate(page);
    }
}