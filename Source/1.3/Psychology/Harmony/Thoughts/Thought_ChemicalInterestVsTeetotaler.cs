using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Thought_ChemicalInterestVsTeetotaler), nameof(Thought_ChemicalInterestVsTeetotaler.OpinionOffset))]
    public class Thought_ChemicalInterestVsTeetotaler_Patch
    {
        [HarmonyPostfix]
        public static void OpinionOffset(ref float __result)
        {
            __result = Mathf.CeilToInt(PsychologySettings.traitOpinionMultiplier * __result);
        }
    }
}