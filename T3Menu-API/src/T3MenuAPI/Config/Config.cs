using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;

namespace T3MenuAPI;

public static class Controls_Config
{
    public class Controls_Info
    {
        public string Move { get; set; } = "WASD";
        public string Select { get; set; } = "E";
        public string Back { get; set; } = "Q";
        public string Exit { get; set; } = "ESC";

        public string LeftArrow { get; set; } = "◄";
        public string RightArrow { get; set; } = "►";
    }

    public class MenuButtons
    {
        public string ScrollUpButton { get; set; } = "W";
        public string ScrollDownButton { get; set; } = "S";
        public string SelectButton { get; set; } = "E";
        public string BackButton { get; set; } = "Shift";
        public string SlideLeftButton { get; set; } = "A";
        public string SlideRightButton { get; set; } = "D";
        public string ExitButton { get; set; } = "R";
    }
    public class Config_Settings
    {
        public bool ShowDeveloperInfo { get; set; } = true;
    }

    public static Controls_Info ControlsInfo { get; private set; } = new Controls_Info();
    public static MenuButtons Buttons { get; private set; } = new MenuButtons();
    public static Config_Settings Settings { get; private set; } = new Config_Settings();

    public static void LoadConfig(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using (var reader = new StreamReader(filePath))
        {
            var config = deserializer.Deserialize<Config>(reader);
            ControlsInfo = config.ControlsInfo;
            Buttons = config.MenuButtons;
            Settings = config.Settings;
        }
    }

    public static void Load()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
        string cfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{assemblyName}";

        LoadConfig($"{cfgPath}/Config.yaml");
    }

    public class Config
    {
        public Controls_Info ControlsInfo { get; set; } = new Controls_Info();
        public MenuButtons MenuButtons { get; set; } = new MenuButtons();
        public Config_Settings Settings { get; set; } = new Config_Settings();
    }
}
