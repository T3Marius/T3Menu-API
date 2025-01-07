# T3Menu-API
T3Menu-API is a plugin created on counterstrikesharp with purpose of creating a better , refined menu controlled with player buttons.

The menu controls are fully confiugarble from config located at **counterstrikesharp/configs/plugins/T3Menu-API/Config.yaml**
# Install
After you extract the T3Menu-API folder, Drag&Drop addons folder into game/csgo and you're good to go.

# Creating Menu Tutorial
```C#
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using T3MenuSharedApi;

namespace MenuTest;

public class MenuTest : BasePlugin
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "MenuExample";
    public override string ModuleVersion => "1.0";

    public IT3MenuManager? MenuManager;

    // get the instance
    public IT3MenuManager? GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get();

        return MenuManager;
    }

    public override void Load(bool hotReload)
    {

    }
    [ConsoleCommand("css_menu")]
    public void onmenu(CCSPlayerController player, CommandInfo info)
    {
        // get the manager and check of nullabilty
        var manager = GetMenuManager();
        if (manager == null)
            return;

        // create menu
        var mainMenu = manager.CreateMenu("Menu Test", isSubMenu: false); // you can add freezePlayer, hasSound too but you can disable them from config directly

        // normal option
        mainMenu.Add("Normal Option", (p, option) =>
        {
            player.PrintToChat("Normal option selected");
            manager.CloseMenu(player); // you can close the menu on select if you want (optional) 
        });

        // bool option
        mainMenu.AddBoolOption("Bool Option", defaultValue: true, (p, option) =>
        {
            if (option is IT3Option boolOption)
            {
                bool isEnabled = boolOption.OptionDisplay!.Contains("✔"); // adding this will automaticly show the ✔ and X based on the value.
                player.PrintToChat(isEnabled ? "Enabled" : "Disabled");
            }
        });

        // slider option | you can leave display: empty if you don't want to show anything but the slider.
        mainMenu.AddSliderOption(display: "", minValue: 0, maxValue: 10, step: 1, defaultValue: 0, onSlide: (p, option) =>
        {
            int value = option.SliderValue; // the value player selects
            player.PrintToChat($"Value: {value}"); // for now slider only works with INT, Values.
        });

        // text option
        mainMenu.AddTextOption("<font color='#FFFF00'>THIS IS A TEXT OPTION</font>"); // you can set color like that for example now is yellow

        // creating sub menu
        mainMenu.Add("Sub Menu", (p, option) => // you need to add it as an option to mainMenu first
        {
            var subMenu = manager.CreateMenu("Sub Menu", isSubMenu: true);
            subMenu.ParentMenu = mainMenu; // set the mainMenu as parent menu for subMenu to propely navigate trough them with back button

            subMenu.Add("This is a sub option", (p, option) =>
            {
                player.PrintToChat("This is a sub option");
            });
            manager.OpenSubMenu(player, subMenu); // opening the submenu
        }); ;
        manager.OpenMainMenu(player, mainMenu); // opening mainmenu
    }
}
```

# Current ButtonTypes:
```
Bool
Button
Text
Slider
```

# Config 
```yaml

controlsInfo:
  move: "[W/S]"
  select: "[E]"
  back: "[SHIFT]"
  exit: "[R]"

menuButtons:
  scrollUpButton: "W"
  scrollDownButton: "S"
  selectButton: "E"
  backButton: "Shift"
  slideLeftButton: "A"
  slideRightButton: "D"
  exitButton: "R"

settings:
  showDeveloperInfo: true


# Button mappings
# Alt1
# Alt2
# Attack
# Attack2
# Attack3
# Bullrush
# Cancel

https://github.com/user-attachments/assets/a8ac4c8d-4aee-4544-bd2f-5ae7ed230ea6


# Duck
# Grenade1
# Grenade2
# Space
# Left
# W
# A
# S
# D
# E
# R
# F
# Shift
# Right
# Run
# Walk
# Weapon1
# Weapon2
# Zoom
# Tab
```
Credits to:

 [@interesting](https://github.com/Interesting-exe) , took example from him with classes

[@ssypchenko](https://github.com/ssypchenko), arrows ideas from him.

 @KitsuneLab Developments, inspired from their menu style: https://github.com/KitsuneLab-Development
# Video
[https://imgur.com/ufu2dI9](https://github.com/user-attachments/assets/a8ac4c8d-4aee-4544-bd2f-5ae7ed230ea6)
