using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.ValueConverters;

public class CommandBarIconConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CommandItem cmd)
            return cmd.Type switch
            {
                CommandType.Play => "mdi play",
                CommandType.Shuffle => "mdi shuffle-variant",
                CommandType.Clear => "mdi close-box-multiple-outline",
                _ => "mdi dots-horizontal"
            };

        return "mdi view-grid";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}