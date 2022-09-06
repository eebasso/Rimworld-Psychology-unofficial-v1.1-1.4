using System;using Verse;using RimWorld;using HarmonyLib;namespace Psychology.Harmony;
[HarmonyPatch(typeof(TraitSet),nameof(TraitSet.GainTrait))]
public static class TraitSet_GainTrait_Patch
{
    [HarmonyPostfix]
    public static void GainTraitPostfix(TraitSet __instance, Trait trait)
    {
        if (!PsycheHelper.TraitDefNamesThatAffectPsyche.Contains(trait.def.defName))
        {
            return;
        }
        if (!PsycheHelper.PsychologyEnabledFast(__instance.pawn))
        {
            return;
        }
        PsycheHelper.Comp(__instance.pawn).Psyche.CalculateAdjustedRatings();
    }
}

public class TraitSet_ConditionalPatches{    public static void GainTrait(TraitSet __instance, Trait trait)    {        PsycheHelper.CorrectTraitsForPawnKinseyEnabled(__instance.pawn);    }}