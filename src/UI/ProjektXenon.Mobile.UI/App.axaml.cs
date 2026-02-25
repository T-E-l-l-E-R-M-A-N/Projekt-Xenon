using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Microsoft.Maui.Controls.Internals;
using ProjektXenon.Mobile.UI.Views;
using ProjektXenon.Shared;
using ProjektXenon.Shared.ViewModels;
using Application = Avalonia.Application;
using Rect = Avalonia.Rect;
using Size = System.Drawing.Size;

namespace ProjektXenon.Mobile.UI;

public partial class App : Application
{
    public static readonly string AppDataPath =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? Environment.CurrentDirectory + "/media"
            : FileSystem.AppDataDirectory + "/media";
    public override void Initialize()
    {
        IoC.Build();
        AvaloniaXamlLoader.Load(this);

        if (!Directory.Exists(AppDataPath))
            Directory.CreateDirectory(AppDataPath);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var viewmodel = IoC.Resolve<MainViewModel>();
        viewmodel.Init();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new Avalonia.Controls.Window
            {
                Width = 320,
                Height = 700,
                CanResize = false,
                DataContext = viewmodel,
                Content = new MainView()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}

public class TilePanel : Panel
{
    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<TilePanel, double>(nameof(CellSize), 96);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<TilePanel, double>(nameof(Spacing), 8);

    /// <summary>
    /// Число колонок (для Orientation=Vertical). Если 0 — auto.
    /// </summary>
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<TilePanel, int>(nameof(Columns), 0);

    /// <summary>
    /// Число строк (для Orientation=Horizontal). Если 0 — auto.
    /// </summary>
    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<TilePanel, int>(nameof(Rows), 0);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<TilePanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<bool> WrapProperty =
        AvaloniaProperty.Register<TilePanel, bool>(nameof(Wrap), true);

    public double CellSize { get => GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }
    public double Spacing { get => GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
    public int Columns { get => GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public int Rows { get => GetValue(RowsProperty); set => SetValue(RowsProperty, value); }
    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public bool Wrap { get => GetValue(WrapProperty); set => SetValue(WrapProperty, value); }

    // Attached свойства: сколько клеток занимает элемент
    public static readonly AttachedProperty<int> ColSpanProperty =
        AvaloniaProperty.RegisterAttached<TilePanel, Control, int>("ColSpan", 1);

    public static readonly AttachedProperty<int> RowSpanProperty =
        AvaloniaProperty.RegisterAttached<TilePanel, Control, int>("RowSpan", 1);

    public static int GetColSpan(AvaloniaObject obj) => obj.GetValue(ColSpanProperty);
    public static void SetColSpan(AvaloniaObject obj, int value) => obj.SetValue(ColSpanProperty, Math.Max(1, value));

    public static int GetRowSpan(AvaloniaObject obj) => obj.GetValue(RowSpanProperty);
    public static void SetRowSpan(AvaloniaObject obj, int value) => obj.SetValue(RowSpanProperty, Math.Max(1, value));

    private readonly Dictionary<Control, (int col, int row, int cs, int rs)> _layout = new();

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
    {
        _layout.Clear();

        double cell = CellSize;
        double gap = Spacing;

        bool flowVertical = Orientation == Orientation.Vertical;

        // Определяем "лимит" по поперечной оси (сколько клеток помещается):
        // Vertical  -> поперечная ось X -> columns
        // Horizontal-> поперечная ось Y -> rows
        int crossLimit = GetCrossLimit(availableSize, cell, gap, flowVertical);

        // Матрица занятости:
        // Vertical  -> список строк, в каждой bool[columns]
        // Horizontal-> список колонок, в каждой bool[rows]
        var occupied = new List<bool[]>();

        int maxColUsed = 0;
        int maxRowUsed = 0;

        foreach (var child in Children)
        {
            if (child is not Control c || !c.IsVisible)
                continue;

            int cs = Math.Max(1, GetColSpan(c));
            int rs = Math.Max(1, GetRowSpan(c));

            // Для предотвращения тупиков: если поперечный лимит меньше спана — ужмём по поперечной оси.
            if (flowVertical) cs = Math.Min(cs, crossLimit);
            else rs = Math.Min(rs, crossLimit);

            // Ищем позицию
            var (col, row) = FindSpot(
                crossLimit: crossLimit,
                colSpan: cs,
                rowSpan: rs,
                occupied: occupied,
                flowVertical: flowVertical);

            MarkOccupied(col, row, cs, rs, occupied, flowVertical, crossLimit);

            _layout[child] = (col, row, cs, rs);

            maxColUsed = Math.Max(maxColUsed, col + cs);
            maxRowUsed = Math.Max(maxRowUsed, row + rs);

            // Размер в пикселях
            var w = cs * cell + (cs - 1) * gap;
            var h = rs * cell + (rs - 1) * gap;
            child.Measure(new Avalonia.Size(w, h));
        }

        // Desired size:
        // Vertical: ширина = crossLimit, высота = maxRowUsed
        // Horizontal: высота = crossLimit, ширина = maxColUsed
        int desiredCols = flowVertical ? crossLimit : maxColUsed;
        int desiredRows = flowVertical ? maxRowUsed : crossLimit;

        double desiredWidth = desiredCols * cell + Math.Max(0, desiredCols - 1) * gap;
        double desiredHeight = desiredRows * cell + Math.Max(0, desiredRows - 1) * gap;

        double finalW = double.IsInfinity(availableSize.Width) ? desiredWidth : Math.Min(desiredWidth, availableSize.Width);
        double finalH = double.IsInfinity(availableSize.Height) ? desiredHeight : Math.Min(desiredHeight, availableSize.Height);

        return new Avalonia.Size((int)finalW, (int)finalH);
    }

    protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
    {
        double cell = CellSize;
        double gap = Spacing;

        foreach (var child in Children)
        {
            if (!_layout.TryGetValue(child, out var info))
                continue;

            var (col, row, cs, rs) = info;

            double x = col * (cell + gap);
            double y = row * (cell + gap);

            double w = cs * cell + (cs - 1) * gap;
            double h = rs * cell + (rs - 1) * gap;

            child.Arrange(new Rect(x, y, w, h));
        }

        return finalSize;
    }

    private int GetCrossLimit(Avalonia.Size availableSize, double cell, double gap, bool flowVertical)
    {
        // flowVertical => cross = Width => columns
        // flowHorizontal => cross = Height => rows
        int explicitLimit = flowVertical ? Columns : Rows;

        if (!Wrap)
        {
            // "Без переноса": считаем поперечную ось бесконечной
            return Math.Max(1, explicitLimit > 0 ? explicitLimit : 1_000_000);
        }

        if (explicitLimit > 0)
            return Math.Max(1, explicitLimit);

        double crossAvail = flowVertical ? availableSize.Width : availableSize.Height;

        if (double.IsInfinity(crossAvail) || crossAvail <= 0)
            return 6; // дефолт

        // Сколько клеток помещается: N*(cell) + (N-1)*gap <= crossAvail
        // => N <= (crossAvail + gap)/(cell+gap)
        return Math.Max(1, (int)Math.Floor((crossAvail + gap) / (cell + gap)));
    }

    private static (int col, int row) FindSpot(
        int crossLimit,
        int colSpan,
        int rowSpan,
        List<bool[]> occupied,
        bool flowVertical)
    {
        // Vertical: сканируем row-major (row 0.., col 0..crossLimit-colSpan)
        // Horizontal: сканируем col-major (col 0.., row 0..crossLimit-rowSpan)
        if (flowVertical)
        {
            int row = 0;
            while (true)
            {
                EnsureLines(occupied, row + rowSpan, crossLimit);
                for (int col = 0; col <= crossLimit - colSpan; col++)
                    if (CanPlace(col, row, colSpan, rowSpan, occupied, flowVertical, crossLimit))
                        return (col, row);
                row++;
            }
        }
        else
        {
            int col = 0;
            while (true)
            {
                EnsureLines(occupied, col + colSpan, crossLimit);
                for (int row = 0; row <= crossLimit - rowSpan; row++)
                    if (CanPlace(col, row, colSpan, rowSpan, occupied, flowVertical, crossLimit))
                        return (col, row);
                col++;
            }
        }
    }

    private static bool CanPlace(
        int col,
        int row,
        int colSpan,
        int rowSpan,
        List<bool[]> occupied,
        bool flowVertical,
        int crossLimit)
    {
        // occupied:
        //  Vertical  -> occupied[row][col]
        //  Horizontal-> occupied[col][row]
        for (int dc = 0; dc < colSpan; dc++)
        {
            for (int dr = 0; dr < rowSpan; dr++)
            {
                int c = col + dc;
                int r = row + dr;

                if (flowVertical)
                {
                    if (c >= crossLimit) return false;
                    if (occupied[r][c]) return false;
                }
                else
                {
                    if (r >= crossLimit) return false;
                    if (occupied[c][r]) return false;
                }
            }
        }
        return true;
    }

    private static void MarkOccupied(
        int col,
        int row,
        int colSpan,
        int rowSpan,
        List<bool[]> occupied,
        bool flowVertical,
        int crossLimit)
    {
        for (int dc = 0; dc < colSpan; dc++)
        {
            for (int dr = 0; dr < rowSpan; dr++)
            {
                int c = col + dc;
                int r = row + dr;

                if (flowVertical)
                    occupied[r][c] = true;
                else
                    occupied[c][r] = true;
            }
        }
    }

    private static void EnsureLines(List<bool[]> occupied, int count, int crossLimit)
    {
        while (occupied.Count < count)
            occupied.Add(new bool[crossLimit]);
    }
}