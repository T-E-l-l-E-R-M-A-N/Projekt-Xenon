using System.Globalization;
using Avalonia.Markup.Xaml;
using ProjektXenon.Shared.ViewModels;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace ProjektXenon.Mobile.UI.ValueConverters;

public class PageNameLocalizeConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return "mdi view-grid";
        
        return (value as IPage).Type switch
        {
            PageType.Explore => Application.Current?.Resources["ExplorePageTitle"].ToString(),
            PageType.Favorites => Application.Current?.Resources["FavoritesPageTitle"].ToString(),
            PageType.NowPlaying => Application.Current?.Resources["NowPlayingPageTitle"].ToString(),
            PageType.Search => Application.Current?.Resources["SearchPageTitle"].ToString(),
            PageType.Settings => Application.Current?.Resources["SettingsPageTitle"].ToString(),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}