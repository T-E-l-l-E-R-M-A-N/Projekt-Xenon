using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.ValueConverters;

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
public class SelectedViewStyleConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;


    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is IViewStyle item1 && values[1] is IViewStyle item2)
        {
            if (item1.Name == item2.Name)
                return true;
        }

        return false;
    }
}