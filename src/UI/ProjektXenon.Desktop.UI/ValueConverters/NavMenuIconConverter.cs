using System.Globalization;
using Avalonia.Markup.Xaml;
using ProjektXenon.Shared.ViewModels;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace ProjektXenon.Desktop.UI.ValueConverters;

public class NavMenuIconConverter : MarkupExtension, IValueConverter
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
            PageType.Explore => "mdi home",
            PageType.Favorites => "mdi heart",
            PageType.NowPlaying => "mdi play",
            PageType.Search => "mdi magnify"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}