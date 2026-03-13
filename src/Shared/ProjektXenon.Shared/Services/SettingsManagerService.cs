using System.Runtime.InteropServices;
using System.Text.Json;

namespace ProjektXenon.Services;

public class SettingsManagerService
{
    private readonly string _settingsFileName = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? Environment.CurrentDirectory + "/settings.json"
        : FileSystem.AppDataDirectory + "/settings.json";

    public AppSettings Settings { get; private set; } = new();

    public event EventHandler<SettingsModel>? SettingsChanged; 

    public void Load()
    {
        if(!File.Exists(_settingsFileName))
            return;

        var json = File.ReadAllText(_settingsFileName);
        Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions()
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsFileName, json);
    }

    public void SetTheme(string theme)
    {
        Settings.Theme = theme;
        SettingsChanged?.Invoke(this, new SettingsModel(name: "Theme", value: theme));
        Save();
    }

    public void SetLanguage(string language)
    {
        Settings.Language = language;
        SettingsChanged?.Invoke(this, new SettingsModel(name: "Language", value: language));
        Save();
    }
}