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
    [HarmonyPatch(typeof(LovePartnerRelationUtility),nameof(LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
    public static class LovePartnerRelationUtility_GenerationChancePatch
    {
        //[LogPerformance]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void PsychologyFormula(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        {
            /* Throw away the existing result and substitute our own formula. */
            float sexualityFactor = 1f;
            if (PsycheHelper.PsychologyEnabled(generated) && PsycheHelper.PsychologyEnabled(other) && PsychologyBase.ActivateKinsey())
            {
                float kinsey = 3 - PsycheHelper.Comp(generated).Sexuality.kinseyRating;
                float kinsey2 = 3 - PsycheHelper.Comp(other).Sexuality.kinseyRating;
                float homo = (generated.gender == other.gender) ? 1f : -1f;
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
                sexualityFactor *= Mathf.InverseLerp(3f, 0f, kinsey2 * homo);
            }
            else
            {
                sexualityFactor = (generated.gender != other.gender) ? 1f : 0.01f;
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
                existingExLoverFactor = Mathf.Pow(0.2f, (float)exLovers);
            }
            else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
            {
                __result = 0f;
                return;
            }
            float generationChanceAgeFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { generated });
            float generationChanceAgeFactor2 = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeFactor", new[] { typeof(Pawn) }).GetValue<float>(new object[] { other });
            float generationChanceAgeGapFactor = Traverse.Create(typeof(LovePartnerRelationUtility)).Method("GetGenerationChanceAgeGapFactor", new[] { typeof(Pawn), typeof(Pawn), typeof(bool) }).GetValue<float>(new object[] { generated, other, ex });
            float incestFactor = 1f;
            if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                incestFactor = 0.01f;
            }
            //float melaninFactor;
            //if (request.FixedMelanin.HasValue)
            //{
            //    melaninFactor = ChildRelationUtility.GetMelaninSimilarityFactor(request.FixedMelanin.Value, other.story.melanin);
            //}
            //else
            //{
            //    melaninFactor = PawnSkinColors.GetMelaninCommonalityFactor(other.story.melanin);
            //}
            //__result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor * melaninFactor;
            __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor;
        }
    }

    [HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.ChangeSpouseRelationsToExSpouse))]
    public static class LovePartnerRelationUtility_PolygamousSpousePatch
    {
        [HarmonyPrefix]
        internal static bool PolygamousException(Pawn pawn)
        {
            if(pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
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
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void PsychologyFormula(ref float __result, Pawn pawn, Pawn partner)
        {
            __result *= FixPawnPartnerFactor(pawn, partner);
            __result *= FixPawnPartnerFactor(partner, pawn);
        }

        public static float FixPawnPartnerFactor(Pawn pawn, Pawn partner)
        {
            if (!PsycheHelper.PsychologyEnabled(pawn))
            {
                return 1f;
            }
            float factor = 1f;
            // Undo age factor from original formula
            factor *= GenMath.FlatHill(0f, 14f, 16f, 25f, 80f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat);
            // Use adjusted drive factors, which account for age
            factor /= 0.01f + 1.25f * Mathf.Pow(PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive, 2) + 0.25f * PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;

            // Undo secondary lovin chance factor ?
            factor *= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);

            // Chasted pawns will want less lovin
            float pure = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
            factor *= Mathf.Pow(8f, pure - 0.5f);

            return factor;
        }

    }

}