using CounterStrikeSharp.API.Core;
using T3MenuSharedApi;

using static T3MenuAPI.T3MenuAPI;

namespace T3MenuAPI;
public class T3MenuManager : IT3MenuManager
{
    public static readonly Dictionary<IntPtr, IT3MenuManager> ActiveMenus = new();
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
    public void CloseActiveMenu(CCSPlayerController? player)
    {
        if (player == null)
            return;

        if (ActiveMenus.TryGetValue(player.Handle, out var activeMenu))
        {
            activeMenu.CloseMenu(player);
            ActiveMenus.Remove(player.Handle);
        }
    }
    public IT3Menu? GetActiveMenu(CCSPlayerController? player)
    {
        if (player == null)
            return null;
        return ActiveMenus.TryGetValue(player.Handle, out var activeMenu) ? (IT3Menu?)activeMenu : null;
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

        if (Players.ContainsKey(player.Slot) && Players[player.Slot].CurrentMenu is T3Menu currentMenu)
        {
            ((T3Menu)menu).ParentMenu = currentMenu;
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