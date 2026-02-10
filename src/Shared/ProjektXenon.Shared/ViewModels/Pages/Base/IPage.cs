namespace ProjektXenon.Shared.ViewModels;

public interface IPage
{
    string Name { get; }
    PageType Type { get; }
    bool IsAvailable { get; set; }
}