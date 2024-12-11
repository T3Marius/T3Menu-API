using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using T3MenuSharedApi;
using static T3MenuAPI.T3MenuAPI;

namespace T3MenuAPI;
public class T3MenuManager : IT3MenuManager
{
    public void OpenMainMenu(CCSPlayerController? player, IT3Menu? menu)
    {
        if (player == null)
            return;
        Players[player.Slot].OpenMainMenu((T3Menu?)menu);
    }

    public void CloseMenu(CCSPlayerController? player)
    {
        if (player == null)
            return;
        Players[player.Slot].OpenMainMenu(null);
    }

    public void CloseSubMenu(CCSPlayerController? player)
    {
        if (player == null)
            return;
        Players[player.Slot].CloseSubMenu();
    }

    public void CloseAllSubMenus(CCSPlayerController? player)
    {
        if (player == null)
            return;
        Players[player.Slot].CloseAllSubMenus();
    }

    public void OpenSubMenu(CCSPlayerController? player, IT3Menu? menu)
    {
        if (player == null || menu == null)
            return;

        // Check if the player exists in the Players dictionary
        if (Players.ContainsKey(player.Slot) && Players[player.Slot].CurrentMenu is T3Menu currentMenu)
        {
            ((T3Menu)menu).ParentMenu = currentMenu; // Automatically set the parent menu for navigation
        }

        Players[player.Slot].OpenSubMenu(menu);
    }

    public IT3Menu CreateMenu(string title = "", bool showDeveloper = true, bool freezePlayer = true, bool hasSound = true, bool isSubMenu = true)
    {
        T3Menu menu = new T3Menu
        {
            Title = title,
            FreezePlayer = freezePlayer,
            HasSound = hasSound,
            IsSubMenu = isSubMenu,
            showDeveloper = showDeveloper,
        };
        return menu;
    }
}
