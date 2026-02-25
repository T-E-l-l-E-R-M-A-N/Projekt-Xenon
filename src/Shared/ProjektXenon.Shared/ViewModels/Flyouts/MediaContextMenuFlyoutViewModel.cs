using System.Collections.ObjectModel;

namespace ProjektXenon.Shared.ViewModels;

public partial class MediaContextMenuFlyoutViewModel : ViewModelBase
{
    #region Private Fields

    private readonly TrackRepositoryService _trackRepository;

    
    private MainViewModel _mainViewModel;

    #endregion
    
    #region Observable Fields
    
    [ObservableProperty] private bool _isOpen;
    [ObservableProperty] private MediaItem? _media;
    [ObservableProperty] private ObservableCollection<CommandItemModel> _commands;

    #endregion

    #region Constructor

    public MediaContextMenuFlyoutViewModel(MainViewModel mainViewModel, TrackRepositoryService trackRepository)
    {
        _mainViewModel = mainViewModel;
        _trackRepository = trackRepository;
    }

    #endregion

    #region Public Methods

    public void Init()
    {
        Commands = new ObservableCollection<CommandItemModel>()
        {
            new CommandItemModel()
            {
                Name = "Воспроизвести",
                Command = new RelayCommand(() =>
                {
                    _mainViewModel.PlayMediaCommand.Execute(Media);
                    CloseContextMenu();
                }),
                Parameter = _media
                
            },
            new CommandItemModel()
            {
                Name = Media.IsFavorite ? "Удалить из избранного" : "Добавить в избранное",
                Command = new AsyncRelayCommand(async () =>
                {
                    Media.IsFavorite = !(Media as Models.MediaItem).IsFavorite;
                    var favs = await _trackRepository.GetFavoritesAsync();
                    List<Models.MediaItem> list = [];
                    if (favs != null)
                    {
                        list.AddRange(favs);
                    }

                    if ((Media as Models.MediaItem).IsFavorite)
                    {
                        if (list.FirstOrDefault(x => x.Id == Media.Id) == null)
                        {
                            list.Add(Media as Models.MediaItem);
                        }
                    }
                    else
                    {
                        if (list.FirstOrDefault(x => x.Id == Media.Id) is Models.MediaItem media)
                        {
                            list.Remove(media);
                        }
                    }


                    await _trackRepository.ChangeFavoritesAsync(list);
                    CloseContextMenu();
                })
                
            },
            new CommandItemModel()
            {
                Name = $"Поиск {Media.Name}",
                Command = new AsyncRelayCommand(async () => { await _mainViewModel.SearchCommand.ExecuteAsync(Media.Name); CloseContextMenu();}),
                
            },
            new CommandItemModel()
            {
                Name = $"Поиск {Media.Artist}",
                Command = new AsyncRelayCommand(async () => { await _mainViewModel.SearchCommand.ExecuteAsync(Media.Artist); CloseContextMenu();}),
                
            },
            new CommandItemModel()
            {
                Name = "Закрыть",
                Command = CloseContextMenuCommand,
                
            }
        };
    }
    [RelayCommand]

    public void OpenContextMenu(MediaItem mediaItem)
    {
        Media = mediaItem;
        Init();
        IsOpen = true;
    }
    [RelayCommand]

    public void CloseContextMenu()
    {
        Media = null;
        IsOpen = false;
    }

    #endregion
}

public partial class CommandItemModel : ViewModelBase
{
    [ObservableProperty] private string? _name = "";
    public ICommand Command { get; set; }
    public object? Parameter { get; set; }
}