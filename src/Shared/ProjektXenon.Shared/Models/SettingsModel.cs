namespace ProjektXenon.Shared.Models;

public class SettingsModel
{

    public SettingsModel(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; }
    public string Value { get; set; }
}