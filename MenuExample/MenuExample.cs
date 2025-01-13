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

        List<object> numbers = new List<object> { 10, 20, 30 };
        mainMenu.AddSliderOption(display: "", customValues: numbers, defaultValue: 0, onSlide: (p, option) =>
        {
            int value = (int)option.SliderValue!;
            player.PrintToChat($"Value: {value}");
        });
        List<object> text = new List<object> { "test", "another test", "hello" };
        mainMenu.AddSliderOption(display: "", customValues: text, defaultValue: 0, onSlide: (p, option) =>
        {
            string text = (string)option.SliderValue!;
            player.PrintToChat($"Value: {text}");
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
