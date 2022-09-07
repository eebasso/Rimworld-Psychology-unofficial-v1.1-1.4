using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
public static class LovePartnerRelationUtility_GenerationChancePatch
{
    [HarmonyPrefix]
    public static bool LovePartnerRelationGenerationChance(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
    {
        if (!PsycheHelper.PsychologyEnabled(generated) || !PsycheHelper.PsychologyEnabled(other))
        {
            __result = 0f;
            return false;
        }
        SpeciesSettings generatedSettings = PsychologySettings.speciesDict[generated.def.defName];
        SpeciesSettings otherSettings = PsychologySettings.speciesDict[other.def.defName];

        float bioAge1 = generated.ageTracker.AgeBiologicalYearsFloat;
        float bioAge2 = other.ageTracker.AgeBiologicalYearsFloat;
        float minDatingAge1 = generatedSettings.minDatingAge;
        float minDatingAge2 = otherSettings.minDatingAge;
        if (bioAge1 < minDatingAge1 || bioAge2 < minDatingAge2 || minDatingAge1 < 0f || minDatingAge2 < 0f)
        {
            // No underage lover relations
            __result = 0f;
            return false;
        }

        float sexualityFactor = 1f;
        //if (PsychologySettings.enableKinsey)
        if (PsychologySettings.enableKinsey)
        {
            float kinsey = PsycheHelper.Comp(generated).Sexuality.kinseyRating / 3f;
            float kinsey2 = PsycheHelper.Comp(other).Sexuality.kinseyRating / 3f;
            if (generated.gender != other.gender)
            {
                kinsey = 2f - kinsey;
                kinsey2 = 2f - kinsey2;
            }
            sexualityFactor *= Mathf.Clamp01(kinsey);
            sexualityFactor *= Mathf.Clamp01(kinsey2);
            if (sexualityFactor == 0f)
            {
                __result = 0f;
                return false;
            }
        }
        else
        {
            if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
            {
                __result = 0f;
                return false;
            }
            if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
            {
                __result = 0f;
                return false;
            }
            sexualityFactor = (generated.gender == other.gender) ? 0.01f : 1f;
        }

        float existingExLoverFactor = 1f;
        if (ex)
        {
            int exLovers = 0;
            List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
            for (int i = 0; i < directRelations.Count; i++)
            {
                if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
                {
                    exLovers++;
                }
            }
            existingExLoverFactor = Mathf.Pow(0.2f, exLovers);
        }
        else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
        {
            __result = 0f;
            return false;
        }
        bool minAge1IsNotZero = minDatingAge1 != 0f;
        bool minAge2IsNotZero = minDatingAge2 != 0f;
        float generationChanceAgeFactor1 = 1f;
        float generationChanceAgeFactor2 = 1f;
        float generationChanceAgeGapFactor = 1f;
        if (minAge1IsNotZero)
        {
            float scaledBioAge1 = PsycheHelper.DatingAgeToVanilla(bioAge1, minDatingAge1);
            generationChanceAgeFactor1 = GetGenerationChanceAgeFactor(scaledBioAge1);
            if (minAge2IsNotZero)
            {
                float scaledBioAge2 = PsycheHelper.DatingAgeToVanilla(bioAge2, minDatingAge2);
                generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(scaledBioAge2);
                if (generatedSettings.enableAgeGap && otherSettings.enableAgeGap)
                {
                    float scaledChrAge1 = PsycheHelper.DatingAgeToVanilla(generated.ageTracker.AgeChronologicalYearsFloat, minDatingAge1);
                    float scaledChrAge2 = PsycheHelper.DatingAgeToVanilla(other.ageTracker.AgeChronologicalYearsFloat, minDatingAge2);
                    generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(scaledBioAge1, scaledBioAge2, scaledChrAge1, scaledChrAge2, ex);
                }
            }

        }
        else if (minAge2IsNotZero)
        {
            generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(PsycheHelper.DatingAgeToVanilla(bioAge2, minDatingAge2));
        }

        
        float incestFactor = 1f;
        if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
        {
            incestFactor = 0.01f;
        }
        float melaninFactor = !request.FixedMelanin.HasValue ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
        __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor1 * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
        return false;
    }

    public static float GetGenerationChanceAgeFactor(float scaledAge)
    {
        return Mathf.InverseLerp(14f, 27f, scaledAge);
    }

    public static float GetGenerationChanceAgeGapFactor(float scaledBioAge1, float scaledBioAge2, float scaledChrAge1, float scaledChrAge2, bool ex)
    {
        float num = Mathf.Abs(scaledBioAge1 - scaledBioAge2);
        if (ex)
        {
            float num2 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(scaledBioAge2, scaledChrAge1, scaledChrAge2);
            if (num2 >= 0f)
            {
                num = Mathf.Min(num, num2);
            }
            float num3 = MinPossibleAgeGapAtMinAgeToGenerateAsLovers(scaledBioAge1, scaledChrAge2, scaledChrAge1);
            if (num3 >= 0f)
            {
                num = Mathf.Min(num, num3);
            }
        }
        if (num > 40f)
        {
            return 0f;
        }
        return Mathf.Clamp(GenMath.LerpDouble(0f, 20f, 1f, 0.001f, num), 0.001f, 1f);
    }

    public static float MinPossibleAgeGapAtMinAgeToGenerateAsLovers(float scaledBioAge2, float scaledChrAge1, float scaledChrAge2)
    {
        float num = scaledChrAge1 - 14f;
        if (num < 0f)
        {
            Log.Warning("at < 0");
            return 0f;
        }
        float num2 = MaxPossibleBioAgeAt(scaledBioAge2, scaledChrAge2, num);
        float num3 = MinPossibleBioAgeAt(scaledBioAge2, num);
        if (num2 < 0f)
        {
            return -1f;
        }
        if (num2 < 14f)
        {
            return -1f;
        }
        if (num3 <= 14f)
        {
            return 0f;
        }
        return num3 - 14f;
    }

    public static float MaxPossibleBioAgeAt(float myBiologicalAge, float myChronologicalAge, float atChronologicalAge)
    {
        float num = Mathf.Min(myBiologicalAge, myChronologicalAge - atChronologicalAge);
        if (num < 0f)
        {
            return -1f;
        }
        return num;
    }

    public static float MinPossibleBioAgeAt(float myBiologicalAge, float atChronologicalAge)
    {
        return Mathf.Max(myBiologicalAge - atChronologicalAge, 0f);
    }
}

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse))]
public static class LovePartnerRelationUtility_PolygamousSpousePatch
{
    [HarmonyPrefix]
    internal static bool PolygamousException(Pawn pawn)
    {
        if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
        {
            IEnumerable<Pawn> spouses = (from p in pawn.relations.RelatedPawns
                                         where pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, p)
                                         select p);
            foreach (Pawn spousePawn in spouses)
            {
                if (!spousePawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, spousePawn);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, spousePawn);
                }
            }
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
public static class LovePartnerRelationUtility_GetLovinMtbHours
{
    [HarmonyPrefix]
    public static bool GetLovinMtbHours(ref float __result, Pawn pawn, Pawn partner)
    {
        if (!PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(partner))
        {
            __result = -1;
            return false;
            //return true;
        }
        if (pawn.Dead || partner.Dead)
        {
            __result = -1f;
            return false;
        }
        if (DebugSettings.alwaysDoLovin)
        {
            __result = 0.1f;
            return false;
        }
        if (pawn.needs.food.Starving || partner.needs.food.Starving)
        {
            __result = -1f;
            return false;
        }
        if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
        {
            __result = -1f;
            return false;
        }

        float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
        float partnerAge = partner.ageTracker.AgeBiologicalYearsFloat;
        float pawnMinLovinAge = PsychologySettings.speciesDict[pawn.def.defName].minLovinAge;
        float partnerMinLovinAge = PsychologySettings.speciesDict[pawn.def.defName].minLovinAge;
        if (pawnAge < pawnMinLovinAge || partnerAge < partnerMinLovinAge || pawnMinLovinAge < 0f || partnerMinLovinAge < 0f)
        {
            // No underage lovin
            __result = -1f;
            return false;
        }
        float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
        float partnerSexDrive = PsycheHelper.Comp(partner).Sexuality.AdjustedSexDrive;
        if (pawnSexDrive < 0.1f || partnerSexDrive < 0.1f)
        {
            // Asexual pawns avoid lovin
            __result = -1f;
            return false;
        }
        float frequency = 1f;
        frequency *= LovinMtbSinglePawnFactor(pawn);
        frequency *= LovinMtbSinglePawnFactor(partner);
        frequency *= Mathf.Pow(pawnSexDrive, 2f);
        frequency *= Mathf.Pow(partnerSexDrive, 2f);
        frequency *= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);
        frequency *= Mathf.Max(partner.relations.SecondaryLovinChanceFactor(pawn), 0.1f);
        frequency /= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner));
        frequency /= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, partner.relations.OpinionOf(pawn));
        if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicLove))
        {
            frequency *= 4f;
        }
        __result = frequency > 0f ? 12f / frequency : -1f;
        return false;
    }

    public static float LovinMtbSinglePawnFactor(Pawn pawn)
    {
        float num = 1f;
        num *= 1f - pawn.health.hediffSet.PainTotal;
        float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
        if (level < 0.5f)
        {
            num *= level * 2f;
        }
        return num;
    }
}


