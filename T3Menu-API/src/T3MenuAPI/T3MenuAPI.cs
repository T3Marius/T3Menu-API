using CounterStrikeSharp.API.Core;
using static CounterStrikeSharp.API.Core.Listeners;
using T3MenuSharedApi;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using static T3MenuAPI.Classes.Library;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using T3MenuAPI.Classes;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Timers;

namespace T3MenuAPI;

public class Buttons
{
    public static readonly Dictionary<string, PlayerButtons> ButtonMapping = new()
    {
        { "Alt1", PlayerButtons.Alt1 },
        { "Alt2", PlayerButtons.Alt2 },
        { "Attack", PlayerButtons.Attack },
        { "Attack2", PlayerButtons.Attack2 },
        { "Attack3", PlayerButtons.Attack3 },
        { "Bullrush", PlayerButtons.Bullrush },
        { "Cancel", PlayerButtons.Cancel },
        { "Duck", PlayerButtons.Duck },
        { "Grenade1", PlayerButtons.Grenade1 },
        { "Grenade2", PlayerButtons.Grenade2 },
        { "Space", PlayerButtons.Jump },
        { "Left", PlayerButtons.Left },
        { "W", PlayerButtons.Forward },
        { "A", PlayerButtons.Moveleft },
        { "S", PlayerButtons.Back },
        { "D", PlayerButtons.Moveright },
        { "E", PlayerButtons.Use },
        { "R", PlayerButtons.Reload },
        { "F", (PlayerButtons)0x800000000 },
        { "Shift", PlayerButtons.Speed },
        { "Right", PlayerButtons.Right },
        { "Run", PlayerButtons.Run },
        { "Walk", PlayerButtons.Walk },
        { "Weapon1", PlayerButtons.Weapon1 },
        { "Weapon2", PlayerButtons.Weapon2 },
        { "Zoom", PlayerButtons.Zoom },
        { "Tab", (PlayerButtons)8589934592 }
    };
}
[MinimumApiVersion(313)]
public class T3MenuAPI : BasePlugin, IPluginConfig<MenuConfig>
{
    public override string ModuleName => "T3MenuAPI";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "T3Marius";

    public static readonly Dictionary<int, T3MenuPlayer> Players = new();
    public static PluginCapability<IT3MenuManager> T3MenuManagerCapability = new("t3menu:manager");
    public static T3MenuAPI Instance { get; set; } = new T3MenuAPI();
    public static readonly Dictionary<CCSPlayerController, T3Menu> ActiveMenus = new();
    private IT3MenuManager MenuManager = null!;
    public IT3MenuManager GetMenuManager()
    {
        if (MenuManager == null)
            MenuManager = new PluginCapability<IT3MenuManager>("t3menu:manager").Get()!;

        return MenuManager;
    }
    private static readonly ConcurrentDictionary<CCSPlayerController, (PlayerButtons Button, DateTime LastPress, int RepeatCount)> ButtonHoldState = new();
    private const float InitialDelay = 0.5f;
    private const float RepeatDelay = 0.1f;

    public MenuConfig Config { get; set; } = new MenuConfig();
    public void OnConfigParsed(MenuConfig config)
    {
        Config = config;
        Config.Update();
    }
    public override void Load(bool hotReload)
    {
        Instance = this;

        var t3MenuManager = new T3MenuManager();
        Capabilities.RegisterPluginCapability(T3MenuManagerCapability, () => t3MenuManager);

        RegisterListener<OnServerPrecacheResources>((resource) =>
        {
            foreach (var file in Config.Sounds.SoundEventFiles)
            {
                resource.AddResource(file);
            }
        });

        RegisterEventHandler<EventPlayerActivate>((@event, info) =>
        {
            if (@event.Userid != null)
            {
                Players[@event.Userid.Slot] = new T3MenuPlayer
                {
                    player = @event.Userid,
                    Buttons = 0
                };
            }
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            if (@event.Userid != null)
            {
                Players.Remove(@event.Userid.Slot);
                if (_playerSavedSpeed.ContainsKey(@event.Userid))
                    _playerSavedSpeed.Remove(@event.Userid);

                if (_frozenPlayers.Contains(@event.Userid))
                    _frozenPlayers.Remove(@event.Userid);
            }
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (ActiveMenus.TryGetValue(player, out var activeMenu))
                {
                    bool shouldFreeze = activeMenu.FreezePlayer ?? Instance.Config.Settings.FreezePlayer;
                    if (shouldFreeze)
                    {
                        player.Freeze();
                    }
                }
            }

            return HookResult.Continue;
        });
        RegisterListener<OnTick>(OnTick);
        RegisterListener<OnPlayerButtonsChanged>(OnPlayerButtonsChange);
        AddCommandListener("say", OnSayListener, HookMode.Pre);
        AddCommandListener("say_team", OnSayListener, HookMode.Pre);

        if (hotReload)
        {
            foreach (var p in Utilities.GetPlayers())
            {
                Players[p.Slot] = new T3MenuPlayer
                {
                    player = p,
                    Buttons = p.Buttons,
                };
            }
        }
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        RemoveListener<OnTick>(OnTick);
        RegisterListener<OnTick>(OnTick);
    }
    public void OnPlayerButtonsChange(CCSPlayerController player, PlayerButtons oldButtons, PlayerButtons newButtons)
    {
        if (Config.Settings.UseOnTickForButtons)
            return;

        if (!Players.TryGetValue(player.Slot, out var menuPlayer) || menuPlayer.MainMenu == null)
            return;

        DateTime now = DateTime.Now;

        if (newButtons != 0 && oldButtons != newButtons)
        {
            ButtonHoldState[player] = (newButtons, now, 0);
            HandleButtonPress(menuPlayer, newButtons);
        }
        else if (newButtons == 0)
        {
            ButtonHoldState.TryRemove(player, out _);
        }
    }
    public void OnTick()
    {
        DateTime now = DateTime.Now;

        foreach (var pl in Utilities.GetPlayers())
        {
            UpdateFrozenPlayers();
        }

        foreach (var player in Players.Values.Where(p => p.MainMenu != null))
        {
            player.UpdateT3CenterHtml();

            if (Config.Settings.UseOnTickForButtons)
            {
                var controller = player.player!;

                PlayerButtons currentButtons = controller.Buttons;

                if (!ButtonHoldState.TryGetValue(controller, out var holdState))
                {
                    holdState = ((PlayerButtons)0, DateTime.MinValue, 0);
                }

                bool buttonHandled = false;

                if (currentButtons != 0)
                {
                    if (holdState.Button != currentButtons)
                    {
                        if (Config.Settings.UseOnTickForButtons)
                        {
                            ButtonHoldState[controller] = (currentButtons, now, 0);
                            buttonHandled = HandleButtonPress(player, currentButtons);
                        }
                    }
                    else
                    {
                        double totalSeconds = (now - holdState.LastPress).TotalSeconds;
                        if (totalSeconds >= InitialDelay)
                        {
                            int repeatCount = (int)((totalSeconds - InitialDelay) / RepeatDelay);
                            if (repeatCount > holdState.RepeatCount)
                            {
                                buttonHandled = HandleButtonPress(player, currentButtons);
                                ButtonHoldState[controller] = (holdState.Button, holdState.LastPress, repeatCount);
                            }
                        }
                    }
                }
                else
                {
                    ButtonHoldState.TryRemove(controller, out _);
                }

                if (buttonHandled)
                {
                    player.Buttons = currentButtons;
                }
            }
        }
    }
    private bool HandleButtonPress(T3MenuPlayer player, PlayerButtons button)
    {
        bool buttonHandled = false;

        var currentMenu = player.MainMenu;

        if (currentMenu != null && currentMenu is T3Menu menu)
        {
            string scrollUpBtn = menu.GetEffectiveButton("ScrollUpButton");
            string scrollDownBtn = menu.GetEffectiveButton("ScrollDownButton");
            string slideLeftBtn = menu.GetEffectiveButton("SlideLeftButton");
            string slideRightBtn = menu.GetEffectiveButton("SlideRightButton");
            string selectBtn = menu.GetEffectiveButton("SelectButton");
            string backBtn = menu.GetEffectiveButton("BackButton");
            string exitBtn = menu.GetEffectiveButton("ExitButton");

            if (Buttons.ButtonMapping.TryGetValue(scrollUpBtn, out var scrollUpButton) && (button & scrollUpButton) != 0)
            {
                player.ScrollUp();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(scrollDownBtn, out var scrollDownButton) && (button & scrollDownButton) != 0)
            {
                player.ScrollDown();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(slideLeftBtn, out var slideLeftButton) && (button & slideLeftButton) != 0)
            {
                player.SlideLeft();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(slideRightBtn, out var slideRightButton) && (button & slideRightButton) != 0)
            {
                player.SlideRight();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(selectBtn, out var selectButton) && (button & selectButton) != 0)
            {
                player.Choose();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(backBtn, out var backButton) && (button & backButton) != 0)
            {
                player.CloseSubMenu();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(exitBtn, out var exitButton) && (button & exitButton) != 0)
            {
                if (player.player == null)
                {
                    Instance.Logger.LogError("Player menu is null");
                    return false;
                }
                GetMenuManager().InvokeMenuClose(player.player, menu);
                Server.NextFrame(() =>
                {
                    player.Close();
                });
                buttonHandled = true;
            }

        }
        else
        {

            if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.ScrollUpButton, out var scrollUpButton) && (button & scrollUpButton) != 0)
            {
                player.ScrollUp();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.ScrollDownButton, out var scrollDownButton) && (button & scrollDownButton) != 0)
            {
                player.ScrollDown();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.SlideLeftButton, out var slideLeftButton) && (button & slideLeftButton) != 0)
            {
                player.SlideLeft();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.SlideRightButton, out var slideRightButton) && (button & slideRightButton) != 0)
            {
                player.SlideRight();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.SelectButton, out var selectButton) && (button & selectButton) != 0)
            {
                player.Choose();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.BackButton, out var backButton) && (button & backButton) != 0)
            {
                player.CloseSubMenu();
                buttonHandled = true;
            }
            else if (Buttons.ButtonMapping.TryGetValue(Config.Buttons.ExitButton, out var exitButton) && (button & exitButton) != 0)
            {
                if (player.player == null || currentMenu == null)
                {
                    return false;
                }
                GetMenuManager().InvokeMenuClose(player.player, currentMenu);

                Server.NextFrame(() =>
                {
                    player.Close();
                });

                buttonHandled = true;
            }
        }
        return buttonHandled;
    }
    public HookResult OnSayListener(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
            return HookResult.Continue;
        var menuPlayer = Players.Values.FirstOrDefault(p => p.player == player);
        if (menuPlayer == null || !menuPlayer.InputMode || menuPlayer.CurrentInputOption == null)
            return HookResult.Continue;

        if (menuPlayer.MainMenu != null)
        {
            string input = command.ArgString;
            if (string.IsNullOrWhiteSpace(input))
                return HookResult.Handled;

            if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
            {
                menuPlayer.InputMode = false;
                menuPlayer.CurrentInputOption = null;
                menuPlayer.UpdateT3CenterHtml();
                return HookResult.Handled;
            }

            input = input.Replace("\"", "").Trim();

            var inputOption = menuPlayer.CurrentInputOption as T3Option;
            if (inputOption != null)
            {
                string displayPart = inputOption.OptionDisplay!.Split(':')[0];

                inputOption.OptionDisplay = $"{displayPart}: [<font color='#00ff00'>{input}</font>]";

                inputOption.DefaultValue = input;

                inputOption.OnInputSubmit?.Invoke(player, inputOption, input);

                menuPlayer.InputMode = false;
                menuPlayer.CurrentInputOption = null;

                menuPlayer.UpdateT3CenterHtml();
            }
            return HookResult.Handled;
        }
        return HookResult.Continue;
    }
}
