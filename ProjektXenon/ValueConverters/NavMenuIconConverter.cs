using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using ProjektXenon.ViewModels;

namespace ProjektXenon.ValueConverters;

public class NavMenuIconConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IPage page)
            return page.Type switch
            {
                PageType.Explore => "mdi home",
                PageType.Search => "mdi magnify",
                PageType.Favorites => "mdi heart",
                PageType.NowPlaying => "mdi equalizer",
            };

        return "mdi view-grid";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ViewStyleIconConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IViewStyle view)
            return view.Name switch
            {
                "Tiles" => "mdi view-grid",
                "List" => "mdi format-list-text",
                "Table" => "mdi table-of-contents",
                "Carousel" => "mdi view-carousel",
            };

        return "mdi view-grid";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}