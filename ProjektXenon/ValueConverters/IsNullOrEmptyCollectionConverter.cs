using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ProjektXenon.ValueConverters;

public sealed class IsNullOrEmptyCollectionConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider) => this;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // null -> пусто
        if (value is null)
            return true;

        // Самый быстрый путь, если есть Count
        if (value is ICollection c)
            return c.Count == 0;

        if (value is IReadOnlyCollection<object> rocObj)
            return rocObj.Count == 0;

        // Общий путь для IEnumerable
        if (value is IEnumerable e)
        {
            var enumerator = e.GetEnumerator();
            try
            {
                return !enumerator.MoveNext(); // нет ни одного элемента => пусто
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        // Если передали не коллекцию — считаем "пустым"
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}