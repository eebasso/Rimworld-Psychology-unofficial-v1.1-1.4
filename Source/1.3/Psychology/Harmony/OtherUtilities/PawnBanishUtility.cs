using System;
using Verse;
using HarmonyLib;
using RimWorld;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnBanishUtility), nameof(PawnBanishUtility.Banish))]
    public static class PawnBanishUtility_Banish_Patch
    {
        [HarmonyPostfix]
        public static void Banish(Pawn pawn, int tile)
        {
            //if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
            //{
            //    return;
            //}
            MayorUtility.RemoveAllMayorshipsFromPawn(pawn);
        }
    }
}

