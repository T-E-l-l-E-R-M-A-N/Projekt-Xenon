using System.Globalization;
using Avalonia.Markup.Xaml;
using ProjektXenon.Shared.Models;
using IMultiValueConverter = Avalonia.Data.Converters.IMultiValueConverter;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace ProjektXenon.Mobile.UI.ValueConverters;

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
public class RandomTileSizeConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Random.Shared.Next(1, 3);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}