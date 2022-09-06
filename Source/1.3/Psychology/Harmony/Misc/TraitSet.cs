using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
public static class TraitSet_GainTrait_Patch
{
    [HarmonyPostfix]
    public static void GainTrait_AdjustedRatingsPostfix(TraitSet __instance, Trait trait)
    {
        if (!PsycheHelper.TraitDefNamesThatAffectPsyche.Contains(trait.def.defName))
        {
            return;
        }
        if (!PsycheHelper.PsychologyEnabled(__instance.pawn))
        {
            return;
        }
        PsycheHelper.Comp(__instance.pawn).Psyche.CalculateAdjustedRatings();
    }
}

public class TraitSet_ManualPatches
{
    public static bool GainTrait_KinseyEnabledPrefix(TraitSet __instance, Trait trait)
    {
        //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        Pawn pawn = __instance.pawn;
        Pawn_SexualityTracker ps = null;
        if (pawn.GetComp<CompPsychology>() != null && pawn.GetComp<CompPsychology>().IsPsychologyPawn)
        {
            ps = pawn.GetComp<CompPsychology>().Sexuality;
        }
        if (ps == null)
        {
            return true;
        }

        //ToDo: replace this with an XML patch that sets commonality of these traits to zero
        if (trait.def == TraitDefOf.Gay || trait.def == TraitDefOf.Bisexual || trait.def == TraitDefOf.Asexual)
        {
            return false;
        }

        if (ps.romanticDrive < 0.5f && trait.def == TraitDefOfPsychology.Codependent)
        {
            return false;
        }
        if (ps.sexDrive < 0.5f && trait.def == TraitDefOfPsychology.Lecher)
        {
            return false;
        }
        return true;
    }
}

//[HarmonyPatch(typeof(TraitSet), nameof(TraitSet.GainTrait))]
//public static class TraitSet_GainTraitPatch
//{
//    //[LogPerformance]
//    [HarmonyPrefix]
//    public static bool KinseyException(ref TraitSet __instance, Trait trait)
//    {
//        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
//        Pawn_SexualityTracker ps = null;
//        if (pawn.GetComp<CompPsychology>() != null && pawn.GetComp<CompPsychology>().IsPsychologyPawn)
//        {
//            ps = pawn.GetComp<CompPsychology>().Sexuality;
//        }
//        if (ps != null && PsychologySettings.enableKinsey && trait.def == TraitDefOf.Gay)
//        {
//            return false;
//        }
//        if (ps != null && PsychologySettings.enableKinsey && trait.def == TraitDefOf.Bisexual)
//        {
//            return false;
//        }
//        if (ps != null && PsychologySettings.enableKinsey && ps.romanticDrive < 0.5f)
//        {
//            if (trait.def == TraitDefOfPsychology.Codependent)
//            {
//                return false;
//            }
//        }
//        if (ps != null && PsychologySettings.enableKinsey && ps.sexDrive < 0.5f)
//        {
//            if (trait.def == TraitDefOfPsychology.Lecher)
//            {
//                return false;
//            }
//        }
//        /*UP: Remove Asexual Trait*/
//        if (ps != null && PsychologySettings.enableKinsey && trait.def == TraitDefOf.Asexual)
//        {
//            return false;
//        }
//        return true;
//    }
//}


