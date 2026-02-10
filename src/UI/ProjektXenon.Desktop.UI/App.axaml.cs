using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ProjektXenon.Desktop.UI.Views;
using ProjektXenon.Shared;
using ProjektXenon.Shared.ViewModels;
using Application = Avalonia.Application;

namespace ProjektXenon.Desktop.UI;

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
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewmodel
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