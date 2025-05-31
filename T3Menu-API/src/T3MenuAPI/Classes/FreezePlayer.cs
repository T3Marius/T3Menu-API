using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace T3MenuAPI.Classes;

public static class Library
{
    public static void SaveSpeed(this CCSPlayerController player, ref float oldSpeed)
    {
        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        oldSpeed = pawn.VelocityModifier;
    }
    public static void Freeze(this CCSPlayerController player)
    {
        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        pawn.VelocityModifier = 0.0f;
    }
    public static void UnFreeze(this CCSPlayerController player, float oldSpeed)
    {
        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (pawn == null)
            return;

        pawn.VelocityModifier = oldSpeed;
    }
}