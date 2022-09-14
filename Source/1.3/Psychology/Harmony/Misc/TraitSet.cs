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
        if (!PsycheHelper.TraitDefNamesThatAffectPsyche.Contains(trait.def.defName))
        {
            return;
        }
        if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        {
            return;
        }
        PsycheHelper.Comp(___pawn).Psyche.CalculateAdjustedRatings();
    }
}

public class TraitSet_ManualPatches
{
    public static bool GainTrait_KinseyEnabledPrefix(Trait trait, Pawn ___pawn)
    {
        int num = 0;
        if (trait.def == TraitDefOfPsychology.Codependent)
        {
            num = 1;
        }
        else if (trait.def == TraitDefOfPsychology.Lecher)
        {
            num = 2;
        }
        else if (trait.def == TraitDefOf.Asexual)
        {
            Log.Warning("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Asexual = " + ___pawn.Label);
            num = 3;
        }
        else if (trait.def == TraitDefOf.Bisexual)
        {
            Log.Warning("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Bisexual for pawn = " + ___pawn.Label);
            num = 4;
        }
        else if (trait.def == TraitDefOf.Gay)
        {
            Log.Warning("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Gay for pawn = " + ___pawn.Label);
            num = 5;
        }
        else
        {
            return true;
        }
        if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        {
            return false;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        {
            return false;
        }
        if (num == 1)
        {
            PsycheHelper.Comp(___pawn).Sexuality.romanticDrive = Mathf.Max(0.5f, PsycheHelper.Comp(___pawn).Sexuality.romanticDrive);
            return true;
        }
        if (num == 2)
        {
            PsycheHelper.Comp(___pawn).Sexuality.sexDrive = Mathf.Max(0.5f, PsycheHelper.Comp(___pawn).Sexuality.sexDrive);
            return true;
        }
        if (num == 3)
        {
            PsycheHelper.Comp(___pawn).Sexuality.AsexualReroll();
            return false;
        }
        if (num == 4)
        {
            PsycheHelper.Comp(___pawn).Sexuality.BisexualReroll();
            return false;
        }
        if (num == 5)
        {
            PsycheHelper.Comp(___pawn).Sexuality.GayReroll();
            return false;
        }
        Log.Error("GainTrait_KinseyEnabledPrefix, impossible condition reached for pawn = " + ___pawn.Label);
        return true;
    }

    public static void HasTrait_KinseyEnabledPostfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
    {
        int num = 0;
        if (tDef == TraitDefOf.Asexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Asexual for pawn = " + ___pawn.Label);
            num = 1;
        }
        else if (tDef == TraitDefOf.Bisexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Bisexual for pawn = " + ___pawn.Label);
            num = 2;
        }
        else if (tDef == TraitDefOf.Gay)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Gay for pawn = " + ___pawn.Label);
            num = 3;
        }
        else
        {
            return;
        }
        if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        {
            return;
        }
        if (num == 1)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.sexDrive < 0.1f;
            return;
        }
        if (num == 2)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0 && PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 6;
            return;
        }
        if (num == 3)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0;
            return;
        }
        Log.Error("HasTrait_KinseyEnabledPostfix, impossible condition reached");
    }

    public static void HasTrait_KinseyEnabledPostfix2(TraitDef tDef, int degree, Pawn ___pawn, ref bool __result)
    {
        int num = 0;
        if (tDef == TraitDefOf.Asexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Asexual for pawn = " + ___pawn.Label);
            num = 1;
        }
        else if (tDef == TraitDefOf.Bisexual)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Bisexual for pawn = " + ___pawn.Label);
            num = 2;
        }
        else if (tDef == TraitDefOf.Gay)
        {
            Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Gay for pawn = " + ___pawn.Label);
            num = 3;
        }
        else
        {
            return;
        }
        if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        {
            return;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        {
            return;
        }
        if (num == 1)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.sexDrive < 0.1f;
            return;
        }
        if (num == 2)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0 && PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 6;
            return;
        }
        if (num == 3)
        {
            __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0;
            return;
        }
        Log.Error("HasTrait_KinseyEnabledPostfix, impossible condition reached");
    }
}


