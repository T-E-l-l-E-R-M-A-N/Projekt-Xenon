namespace ProjektXenon.Helpers;

public class UILayoutHelper
{
    public bool IsMobileLayout { get; set; }

    public event EventHandler<bool>? LayoutChanged;

    public void ChangeLayout(bool isMobile)
    {
        IsMobileLayout = isMobile;
        LayoutChanged?.Invoke(this, IsMobileLayout);
    }
}