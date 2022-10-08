using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.Notify_RescuedBy))]
    public static class Notify_RescuedBy_BleedingHeartPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void AddBleedingHeartThought(Pawn_RelationsTracker __instance, Pawn rescuer)
        {
            if (rescuer.RaceProps.Humanlike && __instance.canGetRescuedThought)
            {
                rescuer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RescuedBleedingHeart, Traverse.Create(__instance).Field("pawn").GetValue<Pawn>());
            }
        }
    }
        
    [HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
    public static class Pawn_RelationsTracker_LovinChancePatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void PsychologyFormula(Pawn_RelationsTracker __instance, ref float __result, Pawn otherPawn)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                /* Throw away the existing result and substitute our own formula. */
                float ageFactor = 1f;
                float sexualityFactor = 1f;
                float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
                float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
                float identityFactor = 1f;
                if (otherPawn.AnimalOrWildMan())
                {
                    __result = 0f;
                    return;
                }
                if (pawn == otherPawn)
                {
                    identityFactor = 0f;
                }
                if (PsychologyBase.ActivateKinsey())
                {
                    float kinsey = 3 - PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
                    float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
                    sexualityFactor = Mathf.InverseLerp(3f, 0f, kinsey * homo);
                }
                else if (Rand.ValueSeeded(pawn.thingIDNumber ^ 3273711) >= 0.015f)
                {
                    if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender)
                        {
                            __result = 0f;
                            return;
                        }
                    }
                    else if (otherPawn.gender == pawn.gender)
                    {
                        __result = 0f;
                        return;
                    }
                }
                if (pawn.gender == Gender.Male)
                {
                    if (ageBiologicalYearsFloat2 < 16f)
                    {
                        __result = 0f;
                        return;
                    }
                    float min = Mathf.Max(16f, ageBiologicalYearsFloat - 30f);
                    float lower = Mathf.Max(20f, ageBiologicalYearsFloat - 10f);
                    ageFactor = GenMath.FlatHill(0.15f, min, lower, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, 0.15f, ageBiologicalYearsFloat2);
                }
                else if (pawn.gender == Gender.Female)
                {
                    if (ageBiologicalYearsFloat2 < 16f)
                    {
                        __result = 0f;
                        return;
                    }
                    if ((ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 10f) && (!pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded)))
                    {
                        ageFactor *= 0.15f;
                    }
                    if (ageBiologicalYearsFloat2 < ageBiologicalYearsFloat - 3f)
                    {
                        ageFactor *= Mathf.InverseLerp(ageBiologicalYearsFloat - 10f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat2) * 0.3f;
                    }
                    else
                    {
                        ageFactor *= GenMath.FlatHill(0.3f, ageBiologicalYearsFloat - 3f, ageBiologicalYearsFloat, ageBiologicalYearsFloat + 10f, ageBiologicalYearsFloat + 30f, 0.15f, ageBiologicalYearsFloat2);
                    }
                }
                ageFactor = Mathf.Lerp(ageFactor, (1.6f - ageFactor), PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
                float disabilityFactor = 1f;
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
                disabilityFactor *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
                disabilityFactor = Mathf.Lerp(disabilityFactor, (1.6f - disabilityFactor), PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded))
                {
                    ageFactor = 1f;
                    disabilityFactor = 1f;
                }


                float beautyFactor = 1f + (otherPawn.GetStatValue(StatDefOf.PawnBeauty) * 0.25f);


                beautyFactor *= 0.75f + (PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool)/2);

                if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < 1f)
                {
                    /* Pawns who can't see as well can't determine beauty as well. */
                    beautyFactor = Mathf.Pow(beautyFactor, pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight));
                }
                if (PsycheHelper.PsychologyEnabled(pawn) && PsychologyBase.ActivateKinsey() && PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive < 1f)
                {
                    if (beautyFactor < 0.5f)
                    {
                        /* Pawns with low sex drive will care about physical features less. */
                        beautyFactor += 0.1f * Mathf.Lerp(0f, 0.5f, 1f - PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive);
                    }
                    /*beautyFactor = Mathf.Pow(beautyFactor, PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive);*/
                    ageFactor = Mathf.Pow(ageFactor, PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive);
                    disabilityFactor = Mathf.Pow(disabilityFactor, PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive);
                }
                float initiatorYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat);
                float recipientYouthFactor = Mathf.InverseLerp(15f, 18f, ageBiologicalYearsFloat2);
                __result = 1f * sexualityFactor * ageFactor * disabilityFactor * beautyFactor * initiatorYouthFactor * recipientYouthFactor * identityFactor;
            }
        }
    }
}
