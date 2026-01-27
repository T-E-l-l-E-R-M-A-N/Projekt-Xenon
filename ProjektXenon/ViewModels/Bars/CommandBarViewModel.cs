using System.Collections.ObjectModel;
using System.ComponentModel;
using ProjektXenon.Services;

namespace ProjektXenon.ViewModels;

public partial class CommandBarViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    
    [ObservableProperty] private ObservableCollection<CommandItem>? _commands;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isVisible;

    public CommandBarViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    public void Init()
    {
        _mainViewModel.PropertyChanged += MainViewModelOnPropertyChanged;
    }

    [RelayCommand]
    private void Expand()
    {
        IsExpanded = !IsExpanded;
    }

    private void MainViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CurrentPage")
        {
            switch (_mainViewModel.CurrentPage?.Type)
            {
                case PageType.Explore:
                    IsVisible = true;
                    var explorePage = _mainViewModel.CurrentPage as ExplorePageViewModel;
                    Commands = new()
                    {
                        new CommandItem()
                        {
                            Name = "Play all",
                            Command = explorePage.PlayAllCommand,
                            Type = CommandType.Play
                        },
                        new CommandItem()
                        {
                            Name = "Shuffle",
                            Command = explorePage.ShuffleCommand,
                            Type = CommandType.Shuffle
                        }
                    };
                    break;
                case PageType.Favorites:
                    IsVisible = true;
                    var favoritesPage = _mainViewModel.CurrentPage as FavoritesPageViewModel;
                    Commands = new()
                    {
                        new CommandItem()
                        {
                            Name = "Play all",
                            Command = favoritesPage.PlayAllCommand,
                            Type = CommandType.Play
                        },
                        new CommandItem()
                        {
                            Name = "Shuffle",
                            Command = favoritesPage.ShuffleCommand,
                            Type = CommandType.Shuffle
                        },
                        new CommandItem()
                        {
                            Name = "Clear all",
                            Command = new AsyncRelayCommand(async () =>
                            {
                                await IoC.Resolve<TrackRepositoryService>().ChangeFavoritesAsync([]);
                            }),
                            Type = CommandType.Clear
                        }
                    };
                    break;
                default:
                    IsVisible = false;
                    Commands = [];
                    break;
            }
        }
    }
}