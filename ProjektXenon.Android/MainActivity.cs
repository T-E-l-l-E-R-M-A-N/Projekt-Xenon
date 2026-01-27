using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Window;
using AndroidX.Activity;
using Avalonia;
using Avalonia.Android;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using ProjektXenon.ViewModels;

namespace ProjektXenon.Android;

[Activity(
    Label = "ProjektXenon.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        IconProvider.Current.Register<MaterialDesignIconProvider>();
        
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
    
}
