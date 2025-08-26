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