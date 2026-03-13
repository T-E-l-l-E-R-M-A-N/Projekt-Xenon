using System.Globalization;
using Avalonia.Markup.Xaml;
using IValueConverter = Avalonia.Data.Converters.IValueConverter;

namespace ProjektXenon.Mobile.UI.ValueConverters;

public class PageTypeIsConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isEqual = (value).ToString() == parameter.ToString();
        return isEqual;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}