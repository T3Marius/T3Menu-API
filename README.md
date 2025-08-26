# T3Menu-API
T3Menu-API is a plugin created on counterstrikesharp with purpose of creating a better , refined menu controlled with player buttons.

The menu controls are fully confiugarble from config located at **counterstrikesharp/configs/plugins/T3Menu-API/T3Menu-API.toml**
# Install
After you extract the T3Menu-API folder, Drag&Drop addons folder into game/csgo and you're good to go.

# Creating Menu Tutorial
```C#
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using T3MenuSharedApi;

namespace MenuExample;

public class MenuExample : BasePlugin
{
    public override string ModuleAuthor => "T3Marius";
    public override string ModuleName => "T3Menu-Example";
    public override string ModuleVersion => "1.0";
    public int PlayerVotes;
    public IT3MenuManager MenuManager = null!; // we can do this now so we won't need to call GetMenuManager() on each menu.

    public override void Load(bool hotReload)
    {

    }
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        // just get the menu manager OnAllPluginsLoaded and that's it no more GetMenuManager() everytime.
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get() ?? throw new Exception("T3MenuAPI not found.");

    }
    [ConsoleCommand("css_menutest")]
    public void OnTest(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        IT3Menu menu = MenuManager.CreateMenu($"Example Menu | Votes: {PlayerVotes}", isSubMenu: false); // if this isn't a sub menu you don't even need to call this.

        // now you don't even need to do the null check.

        menu.AddOption("Normal Option", (p, o) =>
        {
            p.PrintToChat("This is a normal option!");
        });

        menu.AddOption("Vote Option", (p, o) =>
        {
            p.PrintToChat("You added 1 vote!");
            PlayerVotes++;
            menu.Title = $"Example Menu | Votes: {PlayerVotes}"; // call the title again and then refresh
            MenuManager.Refresh(); // you can also add a repeat if you use manager.Refresh(1) when press it will refresh every second.
        });

        menu.AddBoolOption("Bool Option", defaultValue: true, (p, o) =>
        {
            if (o is IT3Option boolOption)
            {
                bool isEnabled = boolOption.OptionDisplay!.Contains("✔"); // this is how you check if the option is enabled or not.

                if (isEnabled)
                {
                    p.PrintToChat("Bool Option is enabled!");
                }
                else
                {
                    p.PrintToChat("Bool Option is disabled!");
                }
            }
        });

        // prepare a list of objects for the slider option
        List<object> intList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        List<object> stringList = ["day", "week", "month", "year"];

        menu.AddSliderOption("Int List", values: intList, defaultValue: 1, displayItems: 3, (player, option, index) =>
        {
            if (option is IT3Option sliderOption && sliderOption.DefaultValue != null)
            {
                int selectedValue = (int)sliderOption.DefaultValue; // convert the default value to int, this is what player pressed in the slider.
                player.PrintToChat($"Selected int: {selectedValue}");
            }
        });

        menu.AddSliderOption("String List", values: stringList, defaultValue: stringList[0], displayItems: 3, (player, option, index) =>
        {
            if (option is IT3Option sliderOption && sliderOption.DefaultValue != null)
            {
                string selectedValue = (string)sliderOption.DefaultValue; // convert the default value to string, this is what player pressed in the slider.
                player.PrintToChat($"Selected string: {selectedValue}");
            }
        });

        menu.AddInputOption("Input Option", "write something", (player, option, input) =>
        {
            // to get the input is really easy, just need converting. You can convert it to any value.
            string inputMessage = input.ToString();

        }, "Write something in chat or write cancel to cancel it."); // this message will be sended when he selects the input option.

        MenuManager.OpenMainMenu(player, menu); // open the menu using the manager.
    }
    [ConsoleCommand("css_custom_menu")]
    public void CustomMenu(CCSPlayerController player)
    {
        IT3Menu menu = MenuManager.CreateMenu("Override Menu");

        menu.OverrideButton("SelectButton", "F"); // this now override the select button with F.
        // all buttons are in readme

        menu.AddOption("Test Option", (player, option) =>
        {

        });
    }
}
```

# Current OptionTypes:
```
Bool
Button
Text
Slider
Input
```

# Config 
```toml
[Controls]           # Move/Select/Back/Exit will be shown in controls info at the bottom of the menu.
Move = "[W/S]"
Select = "[E]"
Back = "[Shift]"
Exit = "[R]"
LeftArrow = "\u25C4"
RightArrow = "\u25BA"
LeftBracket = "]"
RightBracket = "["

[Buttons]              # controls config
ScrollUpButton = "W"
ScrollDownButton = "S"
SelectButton = "E"
BackButton = "Shift"
SlideLeftButton = "A"
SlideRightButton = "D"
ExitButton = "R"

[Sounds]              # if you wanna use these sounds you don't need to add anything.
ScrollUp = "UI.ButtonRolloverLarge"
ScrollDown = "UI.ButtonRolloverLarge"
Select = "Buttons.snd9"
SlideRight = "UI.ButtonRolloverLarge"
SlideLeft = "UI.ButtonRolloverLarge"
Volume = 0.5          # menu sounds volume
SoundEventFiles = []  # if you have custom sounds, add the soundeventfile path here.

[Settings]
ShowDeveloperInfo = true
```
# All buttons the menu have.
```toml
Alt1
Alt2
Attack
Attack2
Attack3
Bullrush
Cancel
Duck
Grenade1
Grenade2
Space
Left
W
A
S
D
E
R
F
Shift
Right
Run
Walk
Weapon1
Weapon2
Zoom
Tab
```
Credits to:

[@interesting](https://github.com/Interesting-exe) , took example from him with classes

[@ssypchenko](https://github.com/ssypchenko), arrows ideas from him.

[@KitsuneLab Developments](https://github.com/KitsuneLab-Development), inspired from their menu style
# Video
[https://imgur.com/ufu2dI9](https://github.com/user-attachments/assets/a8ac4c8d-4aee-4544-bd2f-5ae7ed230ea6)
