using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(SpouseRelationUtility), nameof(SpouseRelationUtility.RemoveGotMarriedThoughts))]
public static class SpouseRelationUtility_RemoveGotMarriedThoughts_Patch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        Log.Message("SpouseRelationUtility_RemoveGotMarriedThoughts_Patch.Transpiler, start");
        return BreakupHelperMethods.InterdictTryGainAndRemoveMemories(codes);
    }
}

