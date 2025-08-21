using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using T3MenuAPI.Classes;
using T3MenuSharedApi;
using static T3MenuAPI.T3MenuAPI;

namespace T3MenuAPI
{
    public class T3MenuManager : IT3MenuManager
    {
        public static readonly Dictionary<IntPtr, IT3MenuManager> ActiveMenus = new();
        private readonly Dictionary<CCSPlayerController, CounterStrikeSharp.API.Modules.Timers.Timer?> _refreshTimers = new();
        public void Refresh(float repeat, Action? onTick)
        {
            if (_menuToRefresh == null || _playerToRefresh == null)
                return;

            if (_refreshTimers.TryGetValue(_playerToRefresh, out var existingTimer) && existingTimer != null)
            {
                existingTimer.Kill();
                _refreshTimers[_playerToRefresh] = null;
            }

            RefreshForPlayer(_playerToRefresh, _menuToRefresh);

            if (repeat > 0)
            {
                _refreshTimers[_playerToRefresh] = Instance.AddTimer(repeat, () =>
                {
                    if (_playerToRefresh.IsValid && _playerToRefresh.Connected == PlayerConnectedState.PlayerConnected)
                    {
                        onTick?.Invoke();
                        RefreshForPlayer(_playerToRefresh, _menuToRefresh);
                    }
                    else
                    {
                        if (_refreshTimers.TryGetValue(_playerToRefresh, out var timer) && timer != null)
                        {
                            timer.Kill();
                            _refreshTimers[_playerToRefresh] = null;
                        }
                    }
                }, TimerFlags.REPEAT);
            }
        }
        public void Refresh(float repeat = 0)
        {
            if (_menuToRefresh == null || _playerToRefresh == null)
                return;

            if (_refreshTimers.TryGetValue(_playerToRefresh, out var existingTimer) && existingTimer != null)
            {
                existingTimer.Kill();
                _refreshTimers[_playerToRefresh] = null;
            }

            RefreshForPlayer(_playerToRefresh, _menuToRefresh);

            if (repeat > 0)
            {
                _refreshTimers[_playerToRefresh] = Instance.AddTimer(repeat, () =>
                {
                    if (_playerToRefresh.IsValid && _playerToRefresh.Connected == PlayerConnectedState.PlayerConnected)
                    {
                        RefreshForPlayer(_playerToRefresh, _menuToRefresh);
                    }
                    else
                    {
                        if (_refreshTimers.TryGetValue(_playerToRefresh, out var timer) && timer != null)
                        {
                            timer.Kill();
                            _refreshTimers[_playerToRefresh] = null;
                        }
                    }
                }, TimerFlags.REPEAT);
            }
        }

        private CCSPlayerController? _playerToRefresh;
        private IT3Menu? _menuToRefresh;

        private void RefreshForPlayer(CCSPlayerController player, IT3Menu menu)
        {
            if (!Players.ContainsKey(player.Slot) || Players[player.Slot].CurrentMenu != menu)
            {
                if (_refreshTimers.TryGetValue(player, out var timer) && timer != null)
                {
                    timer.Kill();
                    _refreshTimers[player] = null;
                }
                return;
            }

            Players[player.Slot].UpdateCenterHtml();
        }

        public void OpenMainMenu(CCSPlayerController? player, IT3Menu? menu)
        {
            if (player == null)
                return;

            _playerToRefresh = player;
            _menuToRefresh = menu;

            Players[player.Slot].OpenMainMenu((T3Menu?)menu);

            if (menu != null)
            {
                ActiveMenus[player.Handle] = this;
            }
            else
            {
                ActiveMenus.Remove(player.Handle);
            }
        }

        public void CloseMenu(CCSPlayerController? player)
        {
            if (player == null)
                return;

            if (_refreshTimers.TryGetValue(player, out var timer) && timer != null)
            {
                timer.Kill();
                _refreshTimers[player] = null;
            }

            Players[player.Slot].OpenMainMenu(null);
            ActiveMenus.Remove(player.Handle);

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
        public void OpenSubMenu(CCSPlayerController? player, IT3Menu? menu)
        {
            if (player == null || menu == null)
                return;

            if (Players.ContainsKey(player.Slot) && Players[player.Slot].CurrentMenu is T3Menu currentMenu)
            {
                ((T3Menu)menu).ParentMenu = currentMenu;
            }

            _menuToRefresh = menu;
            Players[player.Slot].OpenSubMenu(menu);
        }

        public IT3Menu CreateMenu(string title = "", bool showDeveloper = true, bool freezePlayer = true, bool hasSound = true, bool isSubMenu = false, bool isExitable = true)
        {
            T3Menu menu = new T3Menu
            {
                Title = title,
                FreezePlayer = freezePlayer,
                HasSound = hasSound,
                IsSubMenu = isSubMenu,
                showDeveloper = showDeveloper,
                IsExitable = isExitable

            };
            return menu;
        }
    }
}