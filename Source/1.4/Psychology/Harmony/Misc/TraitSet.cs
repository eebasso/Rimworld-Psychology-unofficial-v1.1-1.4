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
        int num;
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
            Log.Error("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Asexual = " + ___pawn);
            num = 3;
        }
        else if (trait.def == TraitDefOf.Bisexual)
        {
            Log.Error("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Bisexual for pawn = " + ___pawn);
            num = 4;
        }
        else if (trait.def == TraitDefOf.Gay)
        {
            Log.Error("GainTrait_KinseyEnabledPrefix was used for TraitDefOf.Gay for pawn = " + ___pawn);
            num = 5;
        }
        else
        {
            return true;
        }
        if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        {
            Log.Error("GainTrait_KinseyEnabledPrefix, unable to get seed for pawn " + ___pawn + " and trait " + trait);
            return false;
        }
        if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        {
            Log.Error("GainTrait_KinseyEnabledPrefix, PsychologyDisabled for pawn " + ___pawn + " and trait " + trait);
            return false;
        }
        switch (num)
        {
            case 1:
                //Log.Message("GainTrait_KinseyEnabledPrefix, reset romantic drive for pawn " + ___pawn + " and trait " + trait);
                PsycheHelper.Comp(___pawn).Sexuality.romanticDrive = Mathf.Max(0.5f, PsycheHelper.Comp(___pawn).Sexuality.romanticDrive);
                return true;
            case 2:
                //Log.Message("GainTrait_KinseyEnabledPrefix, reset sex drive for pawn " + ___pawn + " and trait " + trait);
                PsycheHelper.Comp(___pawn).Sexuality.sexDrive = Mathf.Max(0.5f, PsycheHelper.Comp(___pawn).Sexuality.sexDrive);
                return true;
            case 3:
                //Log.Message("GainTrait_KinseyEnabledPrefix, reroll sexuality to asexual for pawn " + ___pawn + " and trait " + trait);
                PsycheHelper.Comp(___pawn).Sexuality.AsexualTraitReroll();
                return false;
            case 4:
                //Log.Message("GainTrait_KinseyEnabledPrefix, reroll sexuality to bisexual for pawn " + ___pawn + " and trait " + trait);
                PsycheHelper.Comp(___pawn).Sexuality.BisexualTraitReroll();
                return false;
            case 5:
                //Log.Message("GainTrait_KinseyEnabledPrefix, reroll sexuality to homosexual for pawn " + ___pawn + " and trait " + trait);
                PsycheHelper.Comp(___pawn).Sexuality.GayTraitReroll();
                return false;
            default:
                Log.Error("GainTrait_KinseyEnabledPrefix, impossible condition reached for pawn " + ___pawn + " and trait " + trait);
                return true;
        }
    }

    public static void HasTrait_KinseyEnabledPostfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
    {
        //int num = 0;
        if (tDef == TraitDefOf.Asexual)
        {
            //Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Asexual");
            //num = 1;
        }
        else if (tDef == TraitDefOf.Bisexual)
        {
            //Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Bisexual");
            //num = 2;
        }
        else if (tDef == TraitDefOf.Gay)
        {
            //Log.Warning("HasTrait_KinseyEnabledPostfix was used for TraitDefOf.Gay");
            //num = 3;
        }
        else
        {
            return;
        }
        if (__result)
        {
            Log.Error("HasTrait_KinseyEnabledPostfix, pawn " + ___pawn + " has trait " + tDef);
        }
        //if (PsycheHelper.TryGetPawnSeed(___pawn) != true)
        //{
        //    Log.Warning("HasTrait_KinseyEnabledPostfix, TryGetPawnSeed != true");
        //    return;
        //}
        //if (PsycheHelper.PsychologyEnabled(___pawn) != true)
        //{
        //    Log.Warning("HasTrait_KinseyEnabledPostfix, PsychologyEnabled != true for pawn = " + ___pawn);
        //    return;
        //}
        //switch (num)
        //{
        //    case 1:
        //        Log.Warning("HasTrait_KinseyEnabledPostfix, returned sexDrive < 0.1f for pawn = " + ___pawn);
        //        __result = PsycheHelper.Comp(___pawn).Sexuality.sexDrive < 0.1f;
        //        return;
        //    case 2:
        //        Log.Warning("HasTrait_KinseyEnabledPostfix, returned kinsey != 0,6 for pawn = " + ___pawn);
        //        __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0 && PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 6;
        //        return;
        //    case 3:
        //        Log.Warning("HasTrait_KinseyEnabledPostfix, returned kinsey != 0 for pawn = " + ___pawn);
        //        __result = PsycheHelper.Comp(___pawn).Sexuality.kinseyRating != 0;
        //        return;
        //    default:
        //        Log.Error("HasTrait_KinseyEnabledPostfix, impossible condition reached");
        //        return;
        //}
    }

    public static void HasTrait_KinseyEnabledPostfix2(TraitDef tDef, int degree, Pawn ___pawn, ref bool __result)
    {
        HasTrait_KinseyEnabledPostfix(tDef, ___pawn, ref __result);
    }
}


