using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace T3MenuAPI
{
    public class MenuConfig : BasePluginConfig
    {

        public Controls_Info Controls { get; set; } = new Controls_Info();
        public Menu_Buttons Buttons { get; set; } = new Menu_Buttons();
        public Menu_Sounds Sounds { get; set; } = new Menu_Sounds();
        public Config_Settings Settings { get; set; } = new Config_Settings();
    }
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

    public class Menu_Buttons
    {
        public string ScrollUpButton { get; set; } = "W";
        public string ScrollDownButton { get; set; } = "S";
        public string SelectButton { get; set; } = "E";
        public string BackButton { get; set; } = "Shift";
        public string SlideLeftButton { get; set; } = "A";
        public string SlideRightButton { get; set; } = "D";
        public string ExitButton { get; set; } = "R";
    }
    public class Menu_Sounds
    {
        public string ScrollUp { get; set; } = "UI.ButtonRolloverLarge";
        public string ScrollDown { get; set; } = "UI.ButtonRolloverLarge";
        public string Select { get; set; } = "Buttons.snd9";
        public string SlideRight { get; set; } = "UI.ButtonRolloverLarge";
        public string SlideLeft { get; set; } = "UI.ButtonRolloverLarge";
        public float Volume { get; set; } = 1.0f;
        public List<string> SoundEventFiles { get; set; } = [""];
    }
    public class Config_Settings
    {
        public bool UseOnTickForButtons { get; set; } = true;
        public bool FreezePlayer { get; set; } = true;
        public bool ShowDeveloperInfo { get; set; } = true;
        public string DisabledOptionColor { get; set; } = "White";
        public string TextOptionColor { get; set; } = "White";
        public int MaxTitleLenght { get; set; } = 32;
        public int MaxOptionLenght { get; set; } = 32;
    }
}
