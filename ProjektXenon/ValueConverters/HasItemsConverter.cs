using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.ValueConverters;

public sealed class HasItemsConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return false;

        if (value is ICollection c)
            return c.Count > 0;

        if (value is IEnumerable e)
        {
            var enumerator = e.GetEnumerator();
            try
            {
                return enumerator.MoveNext(); // есть хотя бы один элемент
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}