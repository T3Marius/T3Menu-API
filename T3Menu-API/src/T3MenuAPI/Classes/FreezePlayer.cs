using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace T3MenuAPI.Classes;

public static class Library
{
    public static Dictionary<CCSPlayerController, float> _playerSavedSpeed = new Dictionary<CCSPlayerController, float>();
    public static HashSet<CCSPlayerController> _frozenPlayers = new HashSet<CCSPlayerController>();
    public static void Freeze(this CCSPlayerController player)
    {
        if (player.PlayerPawn.Value == null)
            return;

        if (!_playerSavedSpeed.ContainsKey(player))
        {
            _playerSavedSpeed[player] = player.PlayerPawn.Value.VelocityModifier;
        }

        _frozenPlayers.Add(player);
    }
    public static void Unfreeze(this CCSPlayerController player)
    {
        if (player.PlayerPawn.Value == null)
            return;

        _frozenPlayers.Remove(player);

        if (_playerSavedSpeed.TryGetValue(player, out float originalSpeed))
        {
            Server.NextFrame(() =>
            {
                player.PlayerPawn.Value.VelocityModifier = originalSpeed;
                _playerSavedSpeed.Remove(player);
            });
        }
    }
    public static void UpdateFrozenPlayers()
    {
        foreach (var player in _frozenPlayers)
        {
            var pawn = player.PlayerPawn.Value;

            if (pawn != null)
            {
                pawn.VelocityModifier = 0f;
            }
        }
    }
}