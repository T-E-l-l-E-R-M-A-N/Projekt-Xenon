using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Projektanker.Icons.Avalonia;
using Animation = Avalonia.Animation.Animation;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace ProjektXenon.Desktop.UI;

public partial class ScatterTextControl : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<ScatterTextControl, string?>(nameof(Text), "");

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.Register<ScatterTextControl, TimeSpan>(nameof(Duration), TimeSpan.FromMilliseconds(700));

    public static readonly StyledProperty<double> ScatterMarginProperty =
        AvaloniaProperty.Register<ScatterTextControl, double>(nameof(ScatterMargin), 10);

    public static readonly StyledProperty<bool> AutoStartProperty =
        AvaloniaProperty.Register<ScatterTextControl, bool>(nameof(AutoStart), true);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double ScatterMargin
    {
        get => GetValue(ScatterMarginProperty);
        set => SetValue(ScatterMarginProperty, value);
    }

    public bool AutoStart
    {
        get => GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }

    private readonly Random _rnd = new();
    private readonly List<TextBlock> _chars = new();
    private Size _lastSize;
    private bool _busy;

    public ScatterTextControl()
    {
        InitializeComponent();

        // Перестраиваем при изменении текста
        this.GetObservable(TextProperty).Subscribe(_ => RebuildAndMaybeStart());

        // И при реальном изменении размеров
        this.GetObservable(BoundsProperty).Subscribe(_ =>
        {
            var s = Bounds.Size;
            if (s.Width <= 0 || s.Height <= 0) return;
            if (s == _lastSize) return;
            _lastSize = s;

            RebuildAndMaybeStart();
        });
    }

    private void RebuildAndMaybeStart()
    {
        if (_busy) return;

        Build();

        if (AutoStart)
            _ = StartAsync();
    }

    /// <summary>
    /// Создаёт TextBlock на каждый символ, ставит стартовую позицию у края,
    /// а финальную позицию (в правильной последовательности) сохраняет в Tag.
    /// </summary>
    private void Build()
    {
        if (PART_Canvas is null) return;

        PART_Canvas.Children.Clear();
        _chars.Clear();

        var text = Text ?? "";
        if (text.Length == 0 || Bounds.Width <= 0 || Bounds.Height <= 0) return;

        // 1) Создаём блоки в той же последовательности, что и символы в Text
        var blocks = new List<TextBlock>(text.Length);
        var sizes = new List<Size>(text.Length);

        foreach (var ch in text)
        {
            var tb = new TextBlock
            {
                Text = ch.ToString(),
                Opacity = 1,
                // шрифт/цвет обычно наследуются от родителя (UserControl),
                // но если хочешь — можешь задать их явно здесь
            };

            tb.Measure(Size.Infinity);
            var s = tb.DesiredSize;

            // фолбэк: пробел/нулевая ширина
            if (s.Width <= 0) s = new Size(8, s.Height > 0 ? s.Height : 16);

            blocks.Add(tb);
            sizes.Add(s);
        }

        // 2) Считаем финальные позиции в одну строку по центру
        double totalWidth = sizes.Sum(s => s.Width);
        double maxHeight = sizes.Max(s => s.Height);

        double finalX = (Bounds.Width - totalWidth) / 2.0;
        double finalY = (Bounds.Height - maxHeight) / 2.0;

        // 3) Для каждого символа: старт у края, финал по индексам слева направо
        for (int i = 0; i < blocks.Count; i++)
        {
            var tb = blocks[i];

            var (startX, startY) = RandomEdgePosition(Bounds.Width, Bounds.Height, ScatterMargin);

            // старт
            Canvas.SetLeft(tb, startX);
            Canvas.SetTop(tb, startY);

            // финал — строго по порядку символов
            tb.Tag = new Point(finalX, finalY);

            finalX += sizes[i].Width;

            PART_Canvas.Children.Add(tb);
            _chars.Add(tb);
        }
    }

    public async Task StartAsync()
    {
        if (_busy) return;
        if (_chars.Count == 0) return;

        _busy = true;
        try
        {
            var tasks = new List<Task>(_chars.Count);

            for (int i = 0; i < _chars.Count; i++)
            {
                var tb = _chars[i];
                if (tb.Tag is not Point to) continue;

                double fromX = Canvas.GetLeft(tb);
                double fromY = Canvas.GetTop(tb);

                var delay = TimeSpan.FromMilliseconds(i * 15);

                var anim = new Animation
                {
                    Duration = Duration,
                    Easing = new CubicEaseOut(),
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(0d),
                            Setters =
                            {
                                new Avalonia.Styling.Setter(Canvas.LeftProperty, fromX),
                                new Avalonia.Styling.Setter(Canvas.TopProperty, fromY),
                            }
                        },
                        new KeyFrame
                        {
                            Cue = new Cue(1d),
                            Setters =
                            {
                                new Avalonia.Styling.Setter(Canvas.LeftProperty, to.X),
                                new Avalonia.Styling.Setter(Canvas.TopProperty, to.Y),
                            }
                        }
                    }
                };

                tasks.Add(RunWithDelay(anim, tb, delay));
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            _busy = false;
        }
    }

    private (double x, double y) RandomEdgePosition(double w, double h, double margin)
    {
        int edge = _rnd.Next(4);

        return edge switch
        {
            0 => (_rnd.NextDouble() * Math.Max(1, w), margin),                         // top
            1 => (Math.Max(margin, w - margin), _rnd.NextDouble() * Math.Max(1, h)),  // right
            2 => (_rnd.NextDouble() * Math.Max(1, w), Math.Max(margin, h - margin)),  // bottom
            _ => (margin, _rnd.NextDouble() * Math.Max(1, h)),                        // left
        };
    }

    private static async Task RunWithDelay(Animation animation, Animatable target, TimeSpan delay)
    {
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay);

        await animation.RunAsync(target);
    }
}