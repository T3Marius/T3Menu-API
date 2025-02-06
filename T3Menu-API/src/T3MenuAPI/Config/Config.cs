using System;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;

namespace T3MenuAPI
{
    public static class Controls_Config
    {
        public class Controls_Info
        {
            public string Move { get; set; } = "[W/S]";
            public string Select { get; set; } = "[E]";
            public string Back { get; set; } = "[Shift]";
            public string Exit { get; set; } = "[R]";
            public string LeftArrow { get; set; } = "◄";
            public string RightArrow { get; set; } = "►";
            public string LeftBracket { get; set; } = "]";
            public string RightBracket { get; set; } = "[";
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
        public class MenuSounds
        {
            public string ScrollUp { get; set; } = "Ui/buttonclick.vsnd_c";
            public string ScrollDown { get; set; } = "Ui/buttonclick.vsnd_c";
            public string CloseSubMenu { get; set; } = "Ui/buttonrollover.vsnd_c";
            public string OpenMainMenu { get; set; } = "Ui/buttonrollover.vsnd_c";
            public string Choose { get; set; } = "Ui/buttonrollover.vsnd_c";
            public string SlideRight { get; set; } = "Ui/buttonclick.vsnd_c";
            public string SlideLeft { get; set; } = "Ui/buttonclick.vsnd_c";
        }
        public class Config_Settings
        {
            public bool ShowDeveloperInfo { get; set; } = true;
        }

        public static Controls_Info ControlsInfo { get; private set; } = new Controls_Info();
        public static MenuButtons Buttons { get; private set; } = new MenuButtons();
        public static Config_Settings Settings { get; private set; } = new Config_Settings();
        public static MenuSounds Sounds { get; private set; } = new MenuSounds();

        public class Config
        {
            public Controls_Info ControlsInfo { get; set; } = new Controls_Info();
            public MenuButtons MenuButtons { get; set; } = new MenuButtons();
            public Config_Settings Settings { get; set; } = new Config_Settings();
            public MenuSounds Sounds { get; set; } = new MenuSounds();
        }

        public static void Load()
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
            string cfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{assemblyName}";
            string configFile = $"{cfgPath}/Config.yaml";

            if (!Directory.Exists(cfgPath))
                Directory.CreateDirectory(cfgPath);

            LoadConfig(configFile);
        }

        public static void LoadConfig(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties() // Ignore extra user-added fields
                .Build();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            Config defaultConfig = new Config(); // Create default config

            if (File.Exists(filePath))
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        var loadedConfig = deserializer.Deserialize<Config>(reader);
                        MergeConfigs(defaultConfig, loadedConfig); // Merge values
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Config] Failed to load config: {ex.Message}");
                }
            }

            using (var writer = new StreamWriter(filePath))
            {
                writer.Write(serializer.Serialize(defaultConfig));
            }

            ControlsInfo = defaultConfig.ControlsInfo;
            Buttons = defaultConfig.MenuButtons;
            Settings = defaultConfig.Settings;
            Sounds = defaultConfig.Sounds;
        }

        private static void MergeConfigs(Config defaultConfig, Config loadedConfig)
        {
            defaultConfig.ControlsInfo.Move = loadedConfig.ControlsInfo.Move ?? defaultConfig.ControlsInfo.Move;
            defaultConfig.ControlsInfo.Select = loadedConfig.ControlsInfo.Select ?? defaultConfig.ControlsInfo.Select;
            defaultConfig.ControlsInfo.Back = loadedConfig.ControlsInfo.Back ?? defaultConfig.ControlsInfo.Back;
            defaultConfig.ControlsInfo.Exit = loadedConfig.ControlsInfo.Exit ?? defaultConfig.ControlsInfo.Exit;
            defaultConfig.ControlsInfo.LeftArrow = loadedConfig.ControlsInfo.LeftArrow ?? defaultConfig.ControlsInfo.LeftArrow;
            defaultConfig.ControlsInfo.RightArrow = loadedConfig.ControlsInfo.RightArrow ?? defaultConfig.ControlsInfo.RightArrow;
            defaultConfig.ControlsInfo.LeftBracket = loadedConfig.ControlsInfo.LeftBracket ?? defaultConfig.ControlsInfo.LeftBracket;
            defaultConfig.ControlsInfo.RightBracket = loadedConfig.ControlsInfo.RightBracket ?? defaultConfig.ControlsInfo.RightBracket;

            defaultConfig.MenuButtons.ScrollUpButton = loadedConfig.MenuButtons.ScrollUpButton ?? defaultConfig.MenuButtons.ScrollUpButton;
            defaultConfig.MenuButtons.ScrollDownButton = loadedConfig.MenuButtons.ScrollDownButton ?? defaultConfig.MenuButtons.ScrollDownButton;
            defaultConfig.MenuButtons.SelectButton = loadedConfig.MenuButtons.SelectButton ?? defaultConfig.MenuButtons.SelectButton;
            defaultConfig.MenuButtons.BackButton = loadedConfig.MenuButtons.BackButton ?? defaultConfig.MenuButtons.BackButton;
            defaultConfig.MenuButtons.SlideLeftButton = loadedConfig.MenuButtons.SlideLeftButton ?? defaultConfig.MenuButtons.SlideLeftButton;
            defaultConfig.MenuButtons.SlideRightButton = loadedConfig.MenuButtons.SlideRightButton ?? defaultConfig.MenuButtons.SlideRightButton;
            defaultConfig.MenuButtons.ExitButton = loadedConfig.MenuButtons.ExitButton ?? defaultConfig.MenuButtons.ExitButton;

            defaultConfig.Sounds.ScrollUp = loadedConfig.Sounds.ScrollUp ?? defaultConfig.Sounds.ScrollUp;
            defaultConfig.Sounds.ScrollDown = loadedConfig.Sounds.ScrollDown ?? defaultConfig.Sounds.ScrollDown;
            defaultConfig.Sounds.CloseSubMenu = loadedConfig.Sounds.CloseSubMenu ?? defaultConfig.Sounds.CloseSubMenu;
            defaultConfig.Sounds.OpenMainMenu = loadedConfig.Sounds.OpenMainMenu ?? defaultConfig.Sounds.OpenMainMenu;
            defaultConfig.Sounds.Choose = loadedConfig.Sounds.Choose ?? defaultConfig.Sounds.Choose;
            defaultConfig.Sounds.SlideRight = loadedConfig.Sounds.SlideRight ?? defaultConfig.Sounds.SlideRight;
            defaultConfig.Sounds.SlideLeft = loadedConfig.Sounds.SlideLeft ?? defaultConfig.Sounds.SlideLeft;

            defaultConfig.Settings.ShowDeveloperInfo = loadedConfig.Settings.ShowDeveloperInfo;
        }

    }
}
