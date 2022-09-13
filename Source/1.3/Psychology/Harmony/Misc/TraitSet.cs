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
    public static void GainTrait_AdjustedRatingsPostfix(Trait trait, Pawn ___pawn)
    {
        Pawn pawn = ___pawn;
        if (!PsycheHelper.TraitDefNamesThatAffectPsyche.Contains(trait.def.defName))
        {
            return;
        }
        if (PsycheHelper.TryGetPawnSeed(pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(pawn) != true)
        {
            return;
        }
        PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
    }
}

public class TraitSet_ManualPatches
{
    public static bool GainTrait_KinseyEnabledPrefix(Trait trait, Pawn ___pawn)
    {
        //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        Pawn pawn = ___pawn;
        if (PsycheHelper.TryGetPawnSeed(pawn) != true)
        {
            return true;
        }
        if (PsycheHelper.PsychologyEnabled(pawn) != true)
        {
            return true;
        }
        Pawn_SexualityTracker ps = PsycheHelper.Comp(pawn).Sexuality;
        if (trait.def == TraitDefOf.Asexual)
        {
            ps.sexDrive = 0.10f * Rand.ValueSeeded(11 * PsycheHelper.PawnSeed(pawn) + 8);
            return false;
        }
        if (trait.def == TraitDefOf.Bisexual)
        {
            ps.GenerateKinsey(0f, 0.1f, 1f, 2f, 1f, 0.1f, 0f);
            PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
            return false;
        }
        if (trait.def == TraitDefOf.Gay)
        {
            ps.GenerateKinsey(0f, 0.02f, 0.04f, 0.06f, 0.08f, 1f, 2f);
            PsycheHelper.Comp(pawn).Psyche.CalculateAdjustedRatings();
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

    public static void HasTrait_KinseyEnabledPostfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
    {
        Pawn pawn = ___pawn;
        
        if (PsycheHelper.TryGetPawnSeed(pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(pawn) != true)
        {
            return;
        }
        if (tDef == TraitDefOf.Gay)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Gay for pawn = " + pawn.Label);
            __result = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 0;
            return;
        }
        if (tDef == TraitDefOf.Bisexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Bisexual for pawn = " + pawn.Label);
            __result = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 0 && PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 6;
            return;
        }
        if (tDef == TraitDefOf.Asexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Asexual = " + pawn.Label);
            __result = PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f;
            return;
        }
    }

    public static void HasTrait_KinseyEnabledPostfix2(TraitDef tDef, int degree, Pawn ___pawn, ref bool __result)
    {
        Pawn pawn = ___pawn;
        if (PsycheHelper.TryGetPawnSeed(pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(pawn) != true)
        {
            return;
        }
        if (tDef == TraitDefOf.Gay)
        {
            __result = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 0;
            return;
        }
        if (tDef == TraitDefOf.Bisexual)
        {
            __result = PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 0 && PsycheHelper.Comp(pawn).Sexuality.kinseyRating != 6;
            return;
        }
        if (tDef == TraitDefOf.Asexual)
        {
            __result = PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f;
            return;
        }
    }

}


