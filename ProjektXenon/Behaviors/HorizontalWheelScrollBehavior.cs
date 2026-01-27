using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace ProjektXenon.Behaviors;

public sealed class HorizontalWheelScrollBehavior : Behavior<ScrollViewer>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.PointerWheelChanged += AssociatedObjectOnPointerWheelChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.PointerWheelChanged -= AssociatedObjectOnPointerWheelChanged;
    }

    private void AssociatedObjectOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var currentOffset = AssociatedObject?.Offset;
        
        // Определяем направление и скорость прокрутки
        // Обычно умножаем на коэффициент для плавности
        double scrollAmount = e.Delta.Y * 60; // 40 пикселей за "клик" колеса
        
        // Прокручиваем по горизонтали
        var newOffset = new Vector(
            currentOffset.Value.X- scrollAmount, // Y delta для горизонтальной прокрутки
            currentOffset.Value.Y);
        
        // Ограничиваем прокрутку в пределах допустимого
        newOffset = new Vector(
            Math.Max(0, Math.Min(newOffset.X, AssociatedObject.Extent.Width - AssociatedObject.Viewport.Width)),
            newOffset.Y);
        
        // Применяем новое смещение
        AssociatedObject.Offset = newOffset;
        
        // Помечаем событие как обработанное, чтобы не было вертикальной прокрутки
        e.Handled = true;
    }
}