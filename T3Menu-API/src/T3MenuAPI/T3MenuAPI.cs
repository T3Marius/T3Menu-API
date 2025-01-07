using CounterStrikeSharp.API.Core;
using T3MenuAPI.Classes;
using static CounterStrikeSharp.API.Core.Listeners;
using static T3MenuAPI.Controls_Config;
using T3MenuSharedApi;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using System.Collections.Concurrent;

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

public class T3MenuAPI : BasePlugin
{
    public override string ModuleName => "T3MenuAPI";
    public override string ModuleVersion => "1.0.3";
    public override string ModuleAuthor => "T3Marius";

    public static readonly Dictionary<int, T3MenuPlayer> Players = new();
    public static PluginCapability<IT3MenuManager> T3MenuManagerCapability = new("t3menu:manager");

    // Button Hold States for handling fast scrolling
    private static readonly ConcurrentDictionary<CCSPlayerController, (PlayerButtons Button, DateTime LastPress, int RepeatCount)> ButtonHoldState = new();
    private const float InitialDelay = 0.5f;
    private const float RepeatDelay = 0.1f;

    public override void Load(bool hotReload)
    {
        Controls_Config.Load();
        var t3MenuManager = new T3MenuManager();
        Capabilities.RegisterPluginCapability(T3MenuManagerCapability, () => t3MenuManager);

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
            }
            return HookResult.Continue;
        });

        RegisterListener<OnTick>(OnTick);

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

    public void OnTick()
    {
        DateTime now = DateTime.Now;

        foreach (var player in Players.Values.Where(p => p.MainMenu != null))
        {
            player.UpdateCenterHtml();

            var controller = player.player!;
            PlayerButtons currentButtons = controller.Buttons;

            // Handle Button Hold for Fast Scroll
            if (!ButtonHoldState.TryGetValue(controller, out var holdState))
            {
                holdState = ((PlayerButtons)0, DateTime.MinValue, 0);
            }

            bool buttonHandled = false;

            if (currentButtons != 0)
            {
                if (holdState.Button != currentButtons)
                {
                    ButtonHoldState[controller] = (currentButtons, now, 0);
                    buttonHandled = HandleButtonPress(player, currentButtons);
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

            // Update the player's previous button state
            if (buttonHandled)
            {
                player.Buttons = currentButtons;
            }
        }
    }

    private bool HandleButtonPress(T3MenuPlayer player, PlayerButtons button)
    {
        bool buttonHandled = false;

        if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.ScrollUpButton, out var scrollUpButton) && (button & scrollUpButton) != 0)
        {
            player.ScrollUp();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.ScrollDownButton, out var scrollDownButton) && (button & scrollDownButton) != 0)
        {
            player.ScrollDown();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.SlideLeftButton, out var slideLeftButton) && (button & slideLeftButton) != 0)
        {
            player.SlideLeft();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.SlideRightButton, out var slideRightButton) && (button & slideRightButton) != 0)
        {
            player.SlideRight();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.SelectButton, out var selectButton) && (button & selectButton) != 0)
        {
            player.Choose();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.BackButton, out var backButton) && (button & backButton) != 0)
        {
            player.CloseSubMenu();
            buttonHandled = true;
        }
        else if (Buttons.ButtonMapping.TryGetValue(Controls_Config.Buttons.ExitButton, out var exitButton) && (button & exitButton) != 0)
        {
            player.CloseMenu();
            buttonHandled = true;
        }

        return buttonHandled;
    }
}
