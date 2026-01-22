using Avalonia.Controls;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Views;

public partial class MainView : UserControl
{
    private UILayoutHelper _layoutHelper;
    public MainView()
    {
        InitializeComponent();
        _layoutHelper = IoC.Resolve<UILayoutHelper>();
        _layoutHelper.LayoutChanged += LayoutHelperOnLayoutChanged;
        if (this.Bounds.Width > 600)
        {
            _layoutHelper.ChangeLayout(false);
        }
        else
        {
            _layoutHelper.ChangeLayout(true);
        }
    }

    private void LayoutHelperOnLayoutChanged(object? sender, bool e)
    {
        if (_layoutHelper.IsMobileLayout)
        {
            MobileLayout.IsVisible = true;
            DesktopLayout.IsVisible = false;
        }
        else
        {
            MobileLayout.IsVisible = false;
            DesktopLayout.IsVisible = true;
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (e.NewSize.Width > 600)
        {
            _layoutHelper.ChangeLayout(false);
        }
        else
        {
            _layoutHelper.ChangeLayout(true);
        }
    }
}