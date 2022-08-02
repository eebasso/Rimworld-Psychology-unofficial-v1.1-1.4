using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using RimWorld;

namespace Psychology.Harmony
{
    //[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.IdeoTrackerTick))]
    //public class Pawn_IdeoTracker_IdeoTrackerTick_Patch
    //{
    //    [HarmonyPrefix]
    //    public void IdeoTrackerTick(ref float ___certainty, Pawn ___pawn)
    //    {
    //        if (!___pawn.Destroyed && !___pawn.InMentalState && !Find.IdeoManager.classicMode && PsycheHelper.PsychologyEnabled(___pawn))
    //        {
    //            ___certainty += PsycheHelper.Comp(___pawn).Psyche.CertaintyChangePerTick();
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.CertaintyChangePerDay), MethodType.Getter)]
    public static class Pawn_IdeoTracker_CertaintyChangePerDay_Patch
    {
        [HarmonyPostfix]
        public static void CertaintyChangePerDay(ref float __result, Pawn ___pawn)
        {
            if (!PsycheHelper.PsychologyEnabled(___pawn))
            {
                return;
            }
            __result += PsycheHelper.Comp(___pawn).Psyche.CertaintyChangePerDay();
        }
    }

}

