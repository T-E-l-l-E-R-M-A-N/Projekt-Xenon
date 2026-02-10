using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;
using Slider = Avalonia.Controls.Slider;

namespace ProjektXenon.Desktop.UI.Behaviors;
public sealed class SliderDragCommandBehavior : Avalonia.Xaml.Interactivity.Behavior<Slider>
{
    public static readonly StyledProperty<ICommand?> DragCommandProperty =
        AvaloniaProperty.Register<SliderDragCommandBehavior, ICommand?>(nameof(DragCommand));

    public static readonly StyledProperty<bool> FireOnPointerPressedProperty =
        AvaloniaProperty.Register<SliderDragCommandBehavior, bool>(nameof(FireOnPointerPressed), true);

    public static readonly StyledProperty<bool> FireOnDragProperty =
        AvaloniaProperty.Register<SliderDragCommandBehavior, bool>(nameof(FireOnDrag), true);

    public ICommand? DragCommand
    {
        get => GetValue(DragCommandProperty);
        set => SetValue(DragCommandProperty, value);
    }

    /// <summary>Вызывать команду при клике по треку.</summary>
    public bool FireOnPointerPressed
    {
        get => GetValue(FireOnPointerPressedProperty);
        set => SetValue(FireOnPointerPressedProperty, value);
    }

    /// <summary>Вызывать команду во время перетаскивания.</summary>
    public bool FireOnDrag
    {
        get => GetValue(FireOnDragProperty);
        set => SetValue(FireOnDragProperty, value);
    }

    private bool _isDragging;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null) return;

        // Важно: AddHandler, чтобы ловить события и от дочерних элементов (Thumb).
        AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, OnPointerCaptureLost);
        }

        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null) return;

        if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
            return;

        // Захватываем pointer, чтобы получать Moved даже если курсор ушёл за пределы.
        //AssociatedObject.CapturePointer(e.Pointer);
        _isDragging = true;

        if (FireOnPointerPressed)
            TryExecuteCommand(AssociatedObject.Value);

        // Не блокируем стандартную логику Slider (пусть сам сдвигает thumb)
        // Поэтому НЕ ставим e.Handled = true.
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (!_isDragging) return;

        if (FireOnDrag)
            TryExecuteCommand(AssociatedObject.Value);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject is null) return;
        if (!_isDragging) return;

        _isDragging = false;
        //AssociatedObject.ReleasePointerCapture(e.Pointer);

        // Обычно удобно ещё раз отправить финальное значение
        TryExecuteCommand(AssociatedObject.Value);
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isDragging = false;
    }

    private void TryExecuteCommand(double value)
    {
        var cmd = DragCommand;
        if (cmd is null) return;

        // Передаём double (значение Slider)
        if (cmd.CanExecute(value))
            cmd.Execute(value);
    }
}