using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace T3MenuAPI.Classes;

public static class Library
{
    public static void Freeze(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        playerPawn.ChangeMovetype(MoveType_t.MOVETYPE_OBSOLETE);
    }
    public static void UnFreeze(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        playerPawn.ChangeMovetype(MoveType_t.MOVETYPE_WALK);
    }
    private static void ChangeMovetype(this CBasePlayerPawn pawn, MoveType_t movetype)
    {
        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
}