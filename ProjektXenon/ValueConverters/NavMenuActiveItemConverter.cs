using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using ProjektXenon.ViewModels;

namespace ProjektXenon.ValueConverters;

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