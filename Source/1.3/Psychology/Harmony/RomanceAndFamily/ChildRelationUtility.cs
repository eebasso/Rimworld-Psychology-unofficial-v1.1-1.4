using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(ChildRelationUtility), nameof(ChildRelationUtility.ChanceOfBecomingChildOf))]
public static class ChildRelationUtility_ChanceOfBecomingChildOf_Patch
{
    [HarmonyPostfix]
    public static void ChanceOfBecomingChildOf(ref float __result, Pawn father, Pawn mother, Pawn child)
    {
        if (!PsychologySettings.enableKinsey)
        {
            return;
        }
        /* Kinsey-enabled pawns shouldn't have the Gay trait, so we can just apply the sexuality modifier here. */
        if (father != null && child != null && child.GetFather() == null)
        {
            if (PsycheHelper.PsychologyEnabled(father))
            {
                __result *= Mathf.InverseLerp(6f, 0f, PsycheHelper.Comp(father).Sexuality.kinseyRating);
            }
        }
        if (mother != null && child != null && child.GetMother() == null)
        {
            if (PsycheHelper.PsychologyEnabled(mother))
            {
                __result *= Mathf.InverseLerp(6f, 0f, PsycheHelper.Comp(mother).Sexuality.kinseyRating);
            }
        }
    }
}

[HarmonyPatch(typeof(ChildRelationUtility), "GetParentAgeFactor")]
public static class ChildRelationUtility_GetParentAgeFactor_Patch
{
    [HarmonyPrefix]
    public static bool GetParentAgeFactor(ref float __result, Pawn parent, Pawn child, ref float minAgeToHaveChildren, ref float usualAgeToHaveChildren, ref float maxAgeToHaveChildren)
    {
        SpeciesSettings settings = PsychologySettings.speciesDict[parent.def.defName];
        float minLovinAge = settings.minLovinAge;
        if (!settings.enablePsyche || !parent.ageTracker.CurLifeStage.reproductive || minLovinAge < 0f)
        {
            __result = 0f;
            return false;
        }
        if (minLovinAge == 0f || !settings.enableAgeGap)
        {
            float parentChrAge = parent.ageTracker.AgeChronologicalYearsFloat;
            float childChrAge = child.ageTracker.AgeChronologicalYearsFloat;
            __result = minLovinAge + childChrAge < parentChrAge ? 1f : 0f;
            return false;
        }
        minAgeToHaveChildren = PsycheHelper.DatingAgeToVanilla(minAgeToHaveChildren, minLovinAge);
        usualAgeToHaveChildren = PsycheHelper.DatingAgeToVanilla(usualAgeToHaveChildren, minLovinAge);
        maxAgeToHaveChildren = PsycheHelper.DatingAgeToVanilla(maxAgeToHaveChildren, minLovinAge);
        return true;
    }
}

