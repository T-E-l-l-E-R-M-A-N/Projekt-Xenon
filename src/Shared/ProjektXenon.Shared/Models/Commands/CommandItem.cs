namespace ProjektXenon.Shared.Models;

public partial class CommandItem : ViewModelBase
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private ICommand? _command;
    [ObservableProperty] private object? _parameter;
    [ObservableProperty] private CommandType _type;
}