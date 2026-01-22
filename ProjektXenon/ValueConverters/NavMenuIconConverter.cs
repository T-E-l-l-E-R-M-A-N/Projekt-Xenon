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


public class CurrentMediaIdIsEqualToMediaItemConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;


    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is MediaItem item1 && values[1] is MediaItem item2)
        {
            if (item1.Id == item2.Id)
                return true;
        }

        return false;
    }
}
public class NavMenuActiveItemConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;


    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is IPage item1 && values[1] is IPage item2)
        {
            if (item1.Type == item2.Type)
                return true;
        }

        return false;
    }
}