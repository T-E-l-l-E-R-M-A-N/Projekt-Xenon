using System.Globalization;
using Avalonia.Markup.Xaml;
using ProjektXenon.Shared.Models;
using ProjektXenon.Shared.ViewModels;
using IMultiValueConverter = Avalonia.Data.Converters.IMultiValueConverter;

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