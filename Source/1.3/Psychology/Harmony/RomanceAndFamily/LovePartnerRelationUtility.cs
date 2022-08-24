using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
    public static class LovePartnerRelationUtility_GenerationChancePatch
    {
        [HarmonyPrefix]
        public static bool LovePartnerRelationGenerationChance(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        {
            if (!PsycheHelper.PsychologyEnabled(generated) || !PsycheHelper.PsychologyEnabled(other))
            {
                return true;
            }

            /* Replace with our formula to allow for Kinsey rating */
            if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                __result = 0f;
                return false;
            }
            if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                __result = 0f;
                return false;
            }

            float sexualityFactor = 1f;
            if (PsychologyBase.ActivateKinsey())
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

            float generationChanceAgeFactor = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(generated);
            float generationChanceAgeFactor2 = LovePartnerRelationUtility.GetGenerationChanceAgeFactor(other);
            float generationChanceAgeGapFactor = LovePartnerRelationUtility.GetGenerationChanceAgeGapFactor(generated, other, ex);

            float incestFactor = 1f;
            if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                incestFactor = 0.01f;
            }
            float melaninFactor = !request.FixedMelanin.HasValue ? PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin) : ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
            return false;
        }
        //[HarmonyPriority(Priority.Last)]
        //[HarmonyPostfix]
        //public static void LovePartnerRelationGenerationChance(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        //{
        //    /* Throw away the existing result and substitute our own formula. */
        //    float sexualityFactor = 1f;
        //    if (PsycheHelper.PsychologyEnabled(generated) && PsycheHelper.PsychologyEnabled(other) && PsychologyBase.ActivateKinsey())
        //    {
        //        float kinsey = 3 - PsycheHelper.Comp(generated).Sexuality.kinseyRating;
        //        float kinsey2 = 3 - PsycheHelper.Comp(other).Sexuality.kinseyRating;
        //        float homo = (generated.gender == other.gender) ? 1f : -1f;
        //        sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
        //        sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey2 * homo);
        //    }
        //    else
        //    {
        //        sexualityFactor = (generated.gender != other.gender) ? 1f : 0.01f;
        //    }
        //    float existingExLoverFactor = 1f;
        //    if (ex)
        //    {
        //        int exLovers = 0;
        //        List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
        //        for (int i = 0; i < directRelations.Count; i++)
        //        {
        //            if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
        //            {
        //                exLovers++;
        //            }
        //        }
        //        existingExLoverFactor = Mathf.Pow(0.2f, (float)exLovers);
        //    }
        //    else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
        //    {
        //        __result = 0f;
        //        return;
        //    }
        //    float generationChanceAgeFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { generated });
        //    float generationChanceAgeFactor2 = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { other });
        //    float generationChanceAgeGapFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeGapFactor", new[] { typeof(Pawn), typeof(Pawn), typeof(bool) }).GetValue<float>(new object[] { generated, other, ex });
        //    float incestFactor = 1f;
        //    if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
        //    {
        //        incestFactor = 0.01f;
        //    }
        //    //float melaninFactor;
        //    //if (request.FixedMelanin.HasValue)
        //    //{
        //    //    melaninFactor = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
        //    //}
        //    //else
        //    //{
        //    //    melaninFactor = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
        //    //}
        //    //__result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
        //    __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor;
        //}
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
                return true;
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
            bool pawnAgeless = false;
            bool partnerAgeless = false;
            // Add Android exception here
            // Make "CheckIfAgeless" function and use here and elsewhere
            if ((!pawnAgeless && pawnAge < 16) || !partnerAgeless && partnerAge < 16)
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
            __result = 12f;
            __result *= LovinMtbSinglePawnFactor(pawn);
            __result *= LovinMtbSinglePawnFactor(partner);
            __result /= Mathf.Pow(pawnSexDrive, 2f);
            __result /= Mathf.Pow(partnerSexDrive, 2f);
            __result /= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);
            __result /= Mathf.Max(partner.relations.SecondaryLovinChanceFactor(pawn), 0.1f);
            __result *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner));
            __result *= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, partner.relations.OpinionOf(pawn));
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicLove))
            {
                __result /= 4f;
            }
            return false;
        }

        public static float LovinMtbSinglePawnFactor(Pawn pawn)
        {
            float num = 1f;
            num /= 1f - pawn.health.hediffSet.PainTotal;
            float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
            if (level < 0.5f)
            {
                num /= level * 2f;
            }
            return num;
        }


        //[HarmonyPostfix]
        //[HarmonyPriority(Priority.Last)]
        //public static void PsychologyFormula(ref float __result, Pawn pawn, Pawn partner)
        //{
        //    __result *= FixPawnPartnerFactor(pawn, partner);
        //    __result *= FixPawnPartnerFactor(partner, pawn);
        //}

        //public static float FixPawnPartnerFactor(Pawn pawn, Pawn partner)
        //{
        //    if (!PsycheHelper.PsychologyEnabled(pawn))
        //    {
        //        return 1f;
        //    }
        //    float factor = 1f;
        //    // Undo age factor from original formula
        //    factor *= GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
        //    // Use adjusted drive factors, which account for age
        //    factor /= 0.01f + 1.25f * Mathf.Pow(PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive, 2);// + 0.25f * PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;

        //    // Undo secondary lovin chance factor ?
        //    factor *= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);

        //    // Chasted pawns will want less lovin
        //    float pure = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
        //    factor *= Mathf.Pow(8f, pure - 0.5f);

        //    return factor;
        //}

    }

}