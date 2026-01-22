using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ProjektXenon.Controls;

public sealed class PanoramaCarousel : SelectingItemsControl
{
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<PanoramaCarousel, double>(nameof(Spacing), 0);

    public static readonly StyledProperty<bool> CenterSelectedItemProperty =
        AvaloniaProperty.Register<PanoramaCarousel, bool>(nameof(CenterSelectedItem), true);

    public static readonly StyledProperty<TimeSpan> SlideDurationProperty =
        AvaloniaProperty.Register<PanoramaCarousel, TimeSpan>(
            nameof(SlideDuration),
            TimeSpan.FromMilliseconds(220));

    private static readonly IEasing SlideEasing = new CubicEaseOut();

    private ItemsPresenter? _itemsPresenter;
    private Panel? _itemsPanel;
    private readonly TranslateTransform _translate = new();

    private double _step;       // X-дельта между элементами (0 -> 1)
    private double _itemWidth;  // ширина 0-го элемента
    private bool _templateReady;
    private bool _initialOffsetApplied;
    
    private bool _pressed;
    private bool _swipeHandled;
    private IPointer? _pointer;
    private Point _start;
    private Point _last;

    // Кешируем transition, чтобы обновлять Duration без пересоздания лишнего
    private readonly DoubleTransition _xTransition;


    public PanoramaCarousel()
    {
        _xTransition = new DoubleTransition
        {
            Property = TranslateTransform.XProperty,
            Duration = SlideDuration,
            Easing = new CubicEaseOut()
        };


        // ВАЖНО: на старте не включаем transitions, чтобы не было анимации “с нуля”.
        _translate.Transitions = null;
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public bool CenterSelectedItem
    {
        get => GetValue(CenterSelectedItemProperty);
        set => SetValue(CenterSelectedItemProperty, value);
    }

    public TimeSpan SlideDuration
    {
        get => GetValue(SlideDurationProperty);
        set => SetValue(SlideDurationProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _templateReady = _itemsPresenter != null;

        _initialOffsetApplied = false;
        HookPresenter();
        InvalidateArrange();
        
        PointerPressed += OnPressed;
        PointerMoved += OnMoved;
        PointerReleased += OnReleased;
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);

        if (!_templateReady)
            return;

        EnsurePanelResolved();
        UpdatePanelSpacing();
        RecomputeMetrics();

        // Старт и любые резайзы лучше применять БЕЗ анимации,
        // иначе будет “плыть” при каждом layout.
        ApplyOffset(animate: false);

        // После первого корректного позиционирования включаем transitions,
        // чтобы дальнейшие изменения SelectedIndex анимировались.
        if (!_initialOffsetApplied)
        {
            EnableTransitionsIfNeeded();
            _initialOffsetApplied = true;
        }
        else
        {
            // transitions уже включены, ок
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (!_templateReady)
            return;

        if (change.Property == SlideDurationProperty)
        {
            _xTransition.Duration = SlideDuration;
            return;
        }

        if (change.Property == SpacingProperty)
        {
            UpdatePanelSpacing();
            RecomputeMetrics();
            // spacing меняет target — это логично анимировать (если уже включены transitions)
            ApplyOffset(animate: true);
            return;
        }

        if (change.Property == CenterSelectedItemProperty)
        {
            RecomputeMetrics();
            ApplyOffset(animate: true);
            return;
        }

        if (change.Property == SelectedIndexProperty)
        {
            // Вот это — основной кейс: едем от текущего X к target X
            ApplyOffset(animate: true);
            return;
        }

        if (change.Property == SelectedItemProperty)
        {
            // Вот это — основной кейс: едем от текущего X к target X
            ApplyOffset(animate: true);
            return;
        }
    }

    private void HookPresenter()
    {
        EnsurePanelResolved();

        if (_itemsPanel is null && _itemsPresenter is not null)
        {
            _itemsPresenter.LayoutUpdated -= ItemsPresenterOnLayoutUpdated;
            _itemsPresenter.LayoutUpdated += ItemsPresenterOnLayoutUpdated;
        }
    }

    private void ItemsPresenterOnLayoutUpdated(object? sender, EventArgs e)
    {
        EnsurePanelResolved();
        if (_itemsPanel is null)
            return;

        if (_itemsPresenter is not null)
            _itemsPresenter.LayoutUpdated -= ItemsPresenterOnLayoutUpdated;

        UpdatePanelSpacing();
        RecomputeMetrics();

        // После того как панель реально появилась — ставим позицию без анимации и включаем transitions
        ApplyOffset(animate: false);
        EnableTransitionsIfNeeded();
        _initialOffsetApplied = true;
    }

    private void EnsurePanelResolved()
    {
        if (_itemsPresenter is null)
            return;

        _itemsPanel ??= _itemsPresenter.GetVisualChildren().OfType<Panel>().FirstOrDefault();

        if (_itemsPanel is not null)
        {
            _itemsPanel.RenderTransform = _translate;
            _itemsPanel.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Relative);
        }
    }

    private void EnableTransitionsIfNeeded()
    {
        // Если duration = 0 — по сути “без анимации”
        if (SlideDuration <= TimeSpan.Zero)
        {
            _translate.Transitions = null;
            return;
        }

        // Не создаём каждый раз новый список — просто один раз кладём transition
        if (_translate.Transitions is null)
            _translate.Transitions = new Transitions { _xTransition };
    }

    private void UpdatePanelSpacing()
    {
        if (_itemsPanel is StackPanel sp)
            sp.Spacing = Spacing;
    }

    private void RecomputeMetrics()
    {
        _step = 0;
        _itemWidth = 0;

        if (_itemsPanel is null || _itemsPanel.Children.Count == 0)
            return;

        var c0 = _itemsPanel.Children[0];

        _itemWidth = c0.Bounds.Width;
        if (_itemWidth <= 0.1)
            _itemWidth = c0.DesiredSize.Width;

        if (_itemsPanel.Children.Count >= 2)
        {
            var c1 = _itemsPanel.Children[1];
            var step = c1.Bounds.X - c0.Bounds.X;
            if (step > 0.1)
            {
                _step = step;
                return;
            }
        }

        if (_itemWidth > 0.1)
            _step = _itemWidth + Spacing;
    }

    private double ComputeTargetX()
    {
        if (ItemCount <= 0 || _step <= 0.1 || _itemWidth <= 0.1)
            return 0;

        var idx = Math.Clamp(SelectedIndex, 0, ItemCount - 1);

        if (!CenterSelectedItem)
            return -(idx * _step);

        var viewportW = Bounds.Width;
        return -(idx * _step) + (viewportW - _itemWidth) / 2.0;
    }

    private void ApplyOffset(bool animate)
    {
        if (_itemsPanel is null)
            return;

        RecomputeMetrics();
        var target = ComputeTargetX();

        if (!animate || SlideDuration <= TimeSpan.Zero || !_initialOffsetApplied)
        {
            // Применяем БЕЗ анимации: временно отключаем transitions, ставим X, возвращаем обратно
            var saved = _translate.Transitions;
            _translate.Transitions = null;
            _translate.X = target;
            _translate.Transitions = saved;
        }
        else
        {
            // Применяем С анимацией: transitions уже включены, просто меняем X
            EnableTransitionsIfNeeded();
            _translate.X = target;
        }
    }
    
    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {

        var p = e.GetCurrentPoint(this);

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
        if (!_pressed || _swipeHandled || _pointer != e.Pointer)
            return;

        var pos = e.GetPosition(this);
        _last = pos;

        var dx = pos.X - _start.X;
        var dy = pos.Y - _start.Y;

        // если уходим сильно по вертикали — считаем, что это скролл, и не мешаем
        if (Math.Abs(dy) > 30 && Math.Abs(dy) > Math.Abs(dx))
        {
            return;
        }

        if (Math.Abs(dx) >= 40 && Math.Abs(dy) <= 30)
        {
            // dx < 0 — свайп влево; dx > 0 — вправо
            if (dx < 0)
                FireSwipe(SwipeDirection.Left, dx, dy);
            else
                FireSwipe(SwipeDirection.Right, dx, dy);
        }
    }
    
    private void FireSwipe(SwipeDirection direction, double dx, double dy)
    {
        _swipeHandled = true;
        TryChangeSelectedIndex(direction);
    }

    private void TryChangeSelectedIndex(SwipeDirection direction)
    {

        var idx = this.SelectedIndex;

        if (idx < 0 && this.ItemCount > 0)
            idx = 0;

        if (direction == SwipeDirection.Left)
            idx++;
        else
            idx--;

        // clamp
        if (this.ItemCount > 0)
            idx = Math.Clamp(idx, 0, this.ItemCount - 1);

        this.SelectedIndex = idx;
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {

        if (_pointer == e.Pointer)
        {
            _pressed = false;
            _pointer = null;
            //AssociatedObject.ReleasePointerCapture(e.Pointer);
        }
    }
    
    public enum SwipeDirection
    {
        Left,
        Right
    }

    public sealed record SwipeArgs(SwipeDirection Direction, double DeltaX, double DeltaY);
}
