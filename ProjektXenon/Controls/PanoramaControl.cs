using System;
using System.Collections;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
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

    private double _step; // X-дельта между элементами (0 -> 1)
    private double _itemWidth; // ширина 0-го элемента
    private bool _templateReady;
    private bool _initialOffsetApplied;

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
}

public class HorizontalScrollViewer : ScrollViewer
{
    public double WheelSpeed { get; set; } = 60;

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var currentOffset = this.Offset;
        
        // Определяем направление и скорость прокрутки
        // Обычно умножаем на коэффициент для плавности
        double scrollAmount = e.Delta.Y * WheelSpeed; // 40 пикселей за "клик" колеса
        
        // Прокручиваем по горизонтали
        var newOffset = new Vector(
            currentOffset.X - scrollAmount, // Y delta для горизонтальной прокрутки
            currentOffset.Y);
        
        // Ограничиваем прокрутку в пределах допустимого
        newOffset = new Vector(
            Math.Max(0, Math.Min(newOffset.X, this.Extent.Width - this.Viewport.Width)),
            newOffset.Y);
        
        // Применяем новое смещение
        this.Offset = newOffset;
        
        // Помечаем событие как обработанное, чтобы не было вертикальной прокрутки
        e.Handled = true;
    }
}

public class ProgressRing : TemplatedControl
{
    #region Template Parts

    private Canvas? _canvas;
    private readonly List<Ellipse> _ellipses = new();

    #endregion

    #region Dependency Properties

    /// <summary>
    /// Defines the <see cref="BindableWidth"/> property.
    /// </summary>
    public static readonly DirectProperty<ProgressRing, double> BindableWidthProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, double>(
            nameof(BindableWidth),
            o => o.BindableWidth);

    private double _bindableWidth;

    public double BindableWidth
    {
        get => _bindableWidth;
        private set => SetAndRaise(BindableWidthProperty, ref _bindableWidth, value);
    }

    /// <summary>
    /// Defines the <see cref="IsActive"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(
            nameof(IsActive),
            defaultValue: true);

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsLarge"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLargeProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(
            nameof(IsLarge),
            defaultValue: true);

    public bool IsLarge
    {
        get => GetValue(IsLargeProperty);
        set => SetValue(IsLargeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="MaxSideLength"/> property.
    /// </summary>
    public static readonly DirectProperty<ProgressRing, double> MaxSideLengthProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, double>(
            nameof(MaxSideLength),
            o => o.MaxSideLength);

    private double _maxSideLength;

    public double MaxSideLength
    {
        get => _maxSideLength;
        private set => SetAndRaise(MaxSideLengthProperty, ref _maxSideLength, value);
    }

    /// <summary>
    /// Defines the <see cref="EllipseDiameter"/> property.
    /// </summary>
    public static readonly DirectProperty<ProgressRing, double> EllipseDiameterProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, double>(
            nameof(EllipseDiameter),
            o => o.EllipseDiameter);

    private double _ellipseDiameter;

    public double EllipseDiameter
    {
        get => _ellipseDiameter;
        private set => SetAndRaise(EllipseDiameterProperty, ref _ellipseDiameter, value);
    }

    /// <summary>
    /// Defines the <see cref="EllipseOffset"/> property.
    /// </summary>
    public static readonly DirectProperty<ProgressRing, Thickness> EllipseOffsetProperty =
        AvaloniaProperty.RegisterDirect<ProgressRing, Thickness>(
            nameof(EllipseOffset),
            o => o.EllipseOffset);

    private Thickness _ellipseOffset;

    public Thickness EllipseOffset
    {
        get => _ellipseOffset;
        private set => SetAndRaise(EllipseOffsetProperty, ref _ellipseOffset, value);
    }

    /// <summary>
    /// Defines the <see cref="EllipseDiameterScale"/> property.
    /// </summary>
    public static readonly StyledProperty<double> EllipseDiameterScaleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(
            nameof(EllipseDiameterScale),
            defaultValue: 1.0);

    public double EllipseDiameterScale
    {
        get => GetValue(EllipseDiameterScaleProperty);
        set => SetValue(EllipseDiameterScaleProperty, value);
    }

    #endregion

    #region Private Fields

    private DispatcherTimer? _animationTimer;
    private int _currentFrame = 0;
    private const int TotalFrames = 347; // 3.47 seconds at 10ms per frame
    private readonly List<double> _ellipseAngles = new();
    private readonly List<double> _ellipseOpacities = new();
    private readonly List<double> _animationDelays = new() { 0, 0.167, 0.334, 0.501, 0.668, 0.835 };

    #endregion

    #region Property Changed Handlers

    private void OnBindableWidthChanged()
    {
        SetEllipseDiameter(BindableWidth);
        SetEllipseOffset(BindableWidth);
        SetMaxSideLength(BindableWidth);
        UpdateEllipsesPositions();
    }

    private void SetMaxSideLength(double width)
    {
        MaxSideLength = width <= 20.0 ? 20.0 : width;
    }

    private void SetEllipseDiameter(double width)
    {
        EllipseDiameter = (width / 8) * EllipseDiameterScale;
    }

    private void SetEllipseOffset(double width)
    {
        EllipseOffset = new Thickness(0, width / 2, 0, 0);
    }

    private void OnIsLargeChanged()
    {
        UpdateEllipsesVisibility();
    }

    private void OnIsActiveChanged()
    {
        if (IsActive)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void OnEllipseDiameterScaleChanged()
    {
        SetEllipseDiameter(BindableWidth);
    }

    #endregion

    #region Animation

    private void StartAnimation()
    {
        if (_animationTimer != null) return;

        InitializeAnimationValues();

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10) // 100 FPS
        };
        _animationTimer.Tick += OnAnimationTick;
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        if (_animationTimer == null) return;

        _animationTimer.Stop();
        _animationTimer.Tick -= OnAnimationTick;
        _animationTimer = null;
    }

    private void InitializeAnimationValues()
    {
        // Initialize starting values based on WPF animation
        _ellipseAngles.Clear();
        _ellipseOpacities.Clear();

        // Starting angles from WPF animation (Active state)
        _ellipseAngles.AddRange(new[] { -110.0, -116.0, -122.0, -128.0, -134.0, -140.0 });

        // Start with all ellipses invisible
        for (int i = 0; i < 6; i++)
        {
            _ellipseOpacities.Add(0);
        }
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _currentFrame++;
        double currentTime = _currentFrame * 0.01; // Convert to seconds

        UpdateAnimationValues(currentTime);
        UpdateEllipses();
    }

    private void UpdateAnimationValues(double currentTime)
    {
        for (int i = 0; i < 6; i++)
        {
            double adjustedTime = (currentTime - _animationDelays[i] + 3.47) % 3.47;

            // Update opacity based on WPF animation timeline
            if (adjustedTime < 3.21)
            {
                _ellipseOpacities[i] = 1.0;
            }
            else if (adjustedTime < 3.22)
            {
                _ellipseOpacities[i] = 0.0;
            }
            else
            {
                _ellipseOpacities[i] = 0.0;
            }

            // Update angle - simplified rotation
            // In a full implementation, you would interpolate between the WPF keyframes
            double progress = (adjustedTime / 3.47) * 360 * 1.5;
            _ellipseAngles[i] = (-110 - i * 6 + progress) % 360;
        }
    }

    #endregion

    #region Ellipse Management

    private void CreateEllipses()
    {
        if (_canvas == null) return;

        _ellipses.Clear();
        _canvas.Children.Clear();

        for (int i = 0; i < 6; i++)
        {
            var ellipse = new Ellipse
            {
                Width = EllipseDiameter,
                Height = EllipseDiameter,
                Fill = Foreground,
                Opacity = 0,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Name = $"E{i + 1}"
            };

            _ellipses.Add(ellipse);
            _canvas.Children.Add(ellipse);
        }

        UpdateEllipsesVisibility();
        UpdateEllipsesPositions();
    }

    private void UpdateEllipses()
    {
        if (_ellipses.Count == 0 || _canvas == null) return;

        var centerX = _canvas.Bounds.Width / 2;
        var centerY = _canvas.Bounds.Height / 2;
        var radius = Math.Min(centerX, centerY) - EllipseDiameter / 2;

        for (int i = 0; i < _ellipses.Count; i++)
        {
            if (i < _ellipseOpacities.Count)
            {
                _ellipses[i].Opacity = _ellipseOpacities[i];

                // Update position based on angle
                double angleRad = _ellipseAngles[i] * Math.PI / 180.0;
                double x = centerX + radius * Math.Sin(angleRad) - EllipseDiameter / 2;
                double y = centerY - radius * Math.Cos(angleRad) - EllipseDiameter / 2;

                Canvas.SetLeft(_ellipses[i], x);
                Canvas.SetTop(_ellipses[i], y);
            }
        }
    }

    private void UpdateEllipsesPositions()
    {
        if (_ellipses.Count == 0 || _canvas == null) return;

        foreach (var ellipse in _ellipses)
        {
            ellipse.Width = EllipseDiameter;
            ellipse.Height = EllipseDiameter;
        }

        UpdateEllipses();
    }

    private void UpdateEllipsesVisibility()
    {
        if (_ellipses.Count == 0) return;

        for (int i = 0; i < _ellipses.Count; i++)
        {
            _ellipses[i].IsVisible = IsLarge || i < 5;
        }
    }

    #endregion

    #region Control Methods

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _canvas = e.NameScope.Find<Canvas>("PART_Canvas");

        if (_canvas != null)
        {
            CreateEllipses();

            if (IsActive)
            {
                StartAnimation();
            }
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        BindableWidth = Bounds.Width;
        OnBindableWidthChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsActiveProperty)
        {
            OnIsActiveChanged();
        }
        else if (change.Property == IsLargeProperty)
        {
            OnIsLargeChanged();
        }
        else if (change.Property == EllipseDiameterScaleProperty)
        {
            OnEllipseDiameterScaleChanged();
        }
        else if (change.Property == ForegroundProperty)
        {
            UpdateEllipsesBrush();
        }
    }

    private void UpdateEllipsesBrush()
    {
        foreach (var ellipse in _ellipses)
        {
            ellipse.Fill = Foreground;
        }
    }

    public ProgressRing()
    {
        SizeChanged += OnSizeChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BindableWidth = Bounds.Width;
        OnBindableWidthChanged();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        StopAnimation();
    }

    #endregion
}

public class HubControl : TemplatedControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<HubControl, string?>(nameof(Title));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<HubControl, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<HubControl, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<HubControl, IDataTemplate?>(nameof(ItemTemplate));

    /// <summary>
    /// Множитель скорости горизонтального скролла колесом (чем больше, тем быстрее).
    /// </summary>
    public static readonly StyledProperty<double> WheelScrollMultiplierProperty =
        AvaloniaProperty.Register<HubControl, double>(nameof(WheelScrollMultiplier), 48);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double WheelScrollMultiplier
    {
        get => GetValue(WheelScrollMultiplierProperty);
        set => SetValue(WheelScrollMultiplierProperty, value);
    }

    private ScrollViewer? _scrollViewer;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_scrollViewer is not null)
            _scrollViewer.PointerWheelChanged -= OnPointerWheelChanged;

        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");

        if (_scrollViewer is not null)
        {
            _scrollViewer.PointerWheelChanged += OnPointerWheelChanged;
            _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (_scrollViewer is null)
            return;

        // Чаще всего колесо даёт Delta.Y. На тачпадах может быть и Delta.X.
        var dx = e.Delta.X;
        var dy = e.Delta.Y;

        // Если есть горизонтальная составляющая — используем её.
        // Иначе крутим по Delta.Y в горизонталь.
        var scrollDelta = Math.Abs(dx) > 0.001 ? dx : -dy;

        if (Math.Abs(scrollDelta) < 0.001)
            return;

        var offset = _scrollViewer.Offset;
        var newX = offset.X + scrollDelta * WheelScrollMultiplier;

        // Clamp (на всякий)
        var maxX = Math.Max(0, _scrollViewer.Extent.Width - _scrollViewer.Viewport.Width);
        if (newX < 0) newX = 0;
        if (newX > maxX) newX = maxX;

        _scrollViewer.Offset = new Vector(newX, 0);

        e.Handled = true;
    }
}