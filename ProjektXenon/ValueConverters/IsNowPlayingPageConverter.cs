using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using ProjektXenon.ViewModels;

namespace ProjektXenon.ValueConverters;

public class IsNowPlayingPageConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;


    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is IPage item1)
        {
            if (item1.Type == PageType.NowPlaying)
                if (values[1] is MediaItem)
                    return true;
            
        }

        return false;
    }
}