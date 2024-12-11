using CounterStrikeSharp.API.Core;

namespace T3MenuSharedApi;
public interface IT3MenuManager
{
    public void OpenMainMenu(CCSPlayerController? player, IT3Menu? menu);
    public void CloseMenu(CCSPlayerController? player);
    public void CloseSubMenu(CCSPlayerController? player);
    public void CloseAllSubMenus(CCSPlayerController? player);
    public void OpenSubMenu(CCSPlayerController? player, IT3Menu? menu);
    public IT3Menu CreateMenu(string title = "", bool showDeveloper = true, bool freezePlayer = true, bool hasSound = true, bool isSubMenu = true);
}