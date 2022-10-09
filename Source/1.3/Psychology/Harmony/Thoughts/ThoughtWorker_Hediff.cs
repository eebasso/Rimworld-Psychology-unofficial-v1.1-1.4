using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(ThoughtWorker_Hediff), "CurrentStateInternal")]
public static class ThoughtWorker_HediffPsychology
{
    [HarmonyPostfix]
    public static void MethadoneHigh(ThoughtWorker_Hediff __instance, ref ThoughtState __result, Pawn p)
    {
        if(__result.StageIndex != ThoughtState.Inactive.StageIndex)
        {
            if (__instance.def.defName.Contains("Withdrawal") && p.health.hediffSet.HasHediff(HediffDefOfPsychology.MethadoneHigh))
            {
                __result = ThoughtState.Inactive;
                return;
            }
        }
    }
}
