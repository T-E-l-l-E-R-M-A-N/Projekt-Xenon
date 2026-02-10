using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Point = Avalonia.Point;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace ProjektXenon.Desktop.UI.Behaviors;

public sealed class SwipeCommandBehavior : Avalonia.Xaml.Interactivity.Behavior<Control>
{
    // Команды
    public static readonly StyledProperty<ICommand?> SwipeLeftCommandProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, ICommand?>(nameof(SwipeLeftCommand));

    public static readonly StyledProperty<ICommand?> SwipeRightCommandProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, ICommand?>(nameof(SwipeRightCommand));

    // Авто-смена SelectedIndex (если нужно)
    public static readonly StyledProperty<bool> AutoChangeSelectedIndexProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, bool>(nameof(AutoChangeSelectedIndex), true);

    // Минимальная дистанция (px) для свайпа
    public static readonly StyledProperty<double> MinSwipeDistanceProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, double>(nameof(MinSwipeDistance), 40);

    // Максимальное отклонение по вертикали (px), чтобы отличить от вертикального скролла
    public static readonly StyledProperty<double> MaxVerticalDriftProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, double>(nameof(MaxVerticalDrift), 25);

    // Таргет, у которого менять SelectedIndex (если не задан — пытаемся использовать AssociatedObject)
    public static readonly StyledProperty<AvaloniaObject?> TargetProperty =
        AvaloniaProperty.Register<SwipeCommandBehavior, AvaloniaObject?>(nameof(Target));

    public ICommand? SwipeLeftCommand
    {
        get => GetValue(SwipeLeftCommandProperty);
        set => SetValue(SwipeLeftCommandProperty, value);
    }

    public ICommand? SwipeRightCommand
    {
        get => GetValue(SwipeRightCommandProperty);
        set => SetValue(SwipeRightCommandProperty, value);
    }

    /// <summary>Если true — при свайпе меняем SelectedIndex у Target/AssociatedObject (если оно SelectingItemsControl).</summary>
    public bool AutoChangeSelectedIndex
    {
        get => GetValue(AutoChangeSelectedIndexProperty);
        set => SetValue(AutoChangeSelectedIndexProperty, value);
    }

    public double MinSwipeDistance
    {
        get => GetValue(MinSwipeDistanceProperty);
        set => SetValue(MinSwipeDistanceProperty, value);
    }

    public double MaxVerticalDrift
    {
        get => GetValue(MaxVerticalDriftProperty);
        set => SetValue(MaxVerticalDriftProperty, value);
    }

    public AvaloniaObject? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public event EventHandler? SwipeBack;
    public event EventHandler? SwipeForward;

    private bool _pressed;
    private bool _swipeHandled;
    private IPointer? _pointer;
    private Point _start;
    private Point _last;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is null) return;

        // Tunnel — чтобы ловить события даже если внутри есть элементы
        (AssociatedObject as Control).PointerPressed += OnPressed;
        (AssociatedObject as Control).PointerMoved += OnMoved;
        (AssociatedObject as Control).PointerReleased += OnReleased;
        (AssociatedObject as Control).PointerCaptureLost += OnCaptureLost;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            (AssociatedObject as Control).PointerPressed -= OnPressed;
            (AssociatedObject as Control).PointerMoved -= OnMoved;
            (AssociatedObject as Control).PointerReleased -= OnReleased;
            (AssociatedObject as Control).PointerCaptureLost -= OnCaptureLost;
        }

        base.OnDetaching();
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is null) return;

        var p = e.GetCurrentPoint(AssociatedObject);

        if (!p.Properties.IsLeftButtonPressed)
            return;

        _pressed = true;
        _swipeHandled = false;
        _pointer = e.Pointer;

        _start = p.Position;
        _last = _start;

        //AssociatedObject.CapturePointer(e.Pointer);
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (!_pressed || _swipeHandled || AssociatedObject is null || _pointer != e.Pointer)
            return;

        var pos = e.GetPosition(AssociatedObject);
        _last = pos;

        var dx = pos.X - _start.X;
        var dy = pos.Y - _start.Y;

        // если уходим сильно по вертикали — считаем, что это скролл, и не мешаем
        if (Math.Abs(dy) > MaxVerticalDrift && Math.Abs(dy) > Math.Abs(dx))
        {
            // отпускаем свайп, чтобы не конфликтовать со ScrollViewer
            CancelSwipe();
            return;
        }

        if (Math.Abs(dx) >= MinSwipeDistance && Math.Abs(dy) <= MaxVerticalDrift)
        {
            // dx < 0 — свайп влево; dx > 0 — вправо
            if (dx < 0)
                FireSwipe(SwipeDirection.Left, dx, dy);
            else
                FireSwipe(SwipeDirection.Right, dx, dy);
        }
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (AssociatedObject is null) return;

        if (_pointer == e.Pointer)
        {
            _pressed = false;
            _pointer = null;
            //AssociatedObject.ReleasePointerCapture(e.Pointer);
        }
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        CancelSwipe();
    }

    private void CancelSwipe()
    {
        _pressed = false;
        _pointer = null;
        _swipeHandled = true; // прекращаем обработку
    }

    private void FireSwipe(SwipeDirection direction, double dx, double dy)
    {
        _swipeHandled = true;

        // 1) Авто-изменение SelectedIndex
        if (AutoChangeSelectedIndex)
            TryChangeSelectedIndex(direction);

        // 2) Команды
        var args = new SwipeArgs(direction, dx, dy);

        var cmd = direction == SwipeDirection.Left ? SwipeLeftCommand : SwipeRightCommand;
        if (cmd is not null && cmd.CanExecute(args))
            cmd.Execute(args);

        if (direction == SwipeDirection.Left)
        {
            SwipeBack?.Invoke(this, EventArgs.Empty);
        }
        else if (direction == SwipeDirection.Right)
        {
            SwipeForward?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TryChangeSelectedIndex(SwipeDirection direction)
    {
        // Target > AssociatedObject
        var target = Target ?? AssociatedObject;

        if (target is not SelectingItemsControl sic)
            return;

        var idx = sic.SelectedIndex;

        if (idx < 0 && sic.ItemCount > 0)
            idx = 0;

        if (direction == SwipeDirection.Left)
            idx++;
        else
            idx--;

        // clamp
        if (sic.ItemCount > 0)
            idx = Math.Clamp(idx, 0, sic.ItemCount - 1);

        sic.SelectedIndex = idx;
    }

    public enum SwipeDirection
    {
        Left,
        Right
    }

    public sealed record SwipeArgs(SwipeDirection Direction, double DeltaX, double DeltaY);
}