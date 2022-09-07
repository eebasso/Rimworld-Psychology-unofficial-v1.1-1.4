using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.Notify_RescuedBy))]
public static class Notify_RescuedBy_BleedingHeartPatch
{
    //[LogPerformance]
    [HarmonyPostfix]
    public static void AddBleedingHeartThought(Pawn_RelationsTracker __instance, Pawn ___pawn, Pawn rescuer)
    {
        if (rescuer.RaceProps.Humanlike && __instance.canGetRescuedThought)
        {
            //rescuer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RescuedBleedingHeart, Traverse.Create(__instance).Field("pawn").GetValue<Pawn>());
            rescuer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RescuedBleedingHeart, ___pawn);
        }
    }
}

[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
public static class Pawn_RelationsTracker_LovinChancePatch
{
    [HarmonyPrefix]
    public static bool SecondaryLovinChanceFactor(Pawn_RelationsTracker __instance, ref float __result, Pawn ___pawn, Pawn otherPawn)
    {
        Pawn pawn = ___pawn;
        bool pawnHasPsyche = PsycheHelper.PsychologyEnabled(pawn);
        bool otherPawnHasPsyche = PsycheHelper.PsychologyEnabled(otherPawn);

        // Disable psyche for a species means no personality or sexuality generated
        // However, should we use the vanilla formula or just diable all dating?
        if (!pawnHasPsyche || !otherPawnHasPsyche)
        {
            __result = 0f; // Disable all dating for pawns with no psyche
            return false;
            //return true // Use the vanilla formula
        }
        if (pawn == otherPawn)
        {
            __result = 0f;
            return false;
        }

        /* SEXUAL PREFERENCE FACTOR */
        float sexualityFactor = 1f;
        /* Psychology result */
        if (PsychologySettings.enableKinsey)
        {
            float pawnHomo = PsycheHelper.Comp(pawn).Sexuality.kinseyRating / 3f;
            sexualityFactor = Mathf.Clamp01(pawn.gender == otherPawn.gender ? pawnHomo : 2f - pawnHomo);
        }
        // Vanilla Asexual, Bisexual, and Gay traits
        else if (pawn.story != null && pawn.story.traits != null)
        {
            if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
            {
                __result = 0f;
                return false;
            }
            if (!pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    if (otherPawn.gender != pawn.gender)
                    {
                        __result = 0f;
                        return false;
                    }
                }
                else if (otherPawn.gender == pawn.gender)
                {
                    __result = 0f;
                    return false;
                }
            }
        }

        /* AGE FACTOR */
        float ageFactor = CalculateAgeFactor(pawn, otherPawn);
        if (ageFactor == 0f)
        {
            __result = 0f;
            return false;
        }

        /* BEAUTY FACTOR */
        float pawnBeauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
        float otherPawnBeauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
        float otherPawnCool = PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool);
        float pawnOpenMinded = pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;

        /* Beautiful pawns will have higher beauty standards. Everyone wants to date out of league */
        float physicalFactor = otherPawnBeauty - 0.75f * pawnBeauty;
        /* Open Minded pawns don't care about physical beauty */
        physicalFactor *= 1f - pawnOpenMinded;
        /* Pawns who can't see as well can't determine physical beauty as well. */
        physicalFactor *= 0.1f + 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);

        /* Cool pawns are more attractive */
        float personalityFactor = 2f * otherPawnCool - 1f;

        // Men will care more about physical beauty, women will care more about personality
        physicalFactor *= pawn.gender == Gender.Male ? 1.6f : 0.6f;
        personalityFactor *= pawn.gender == Gender.Female ? 1.6f : 0.6f;

        /* Turn into multiplicative factor. This ranges between 0.27 and 2.85 */
        float beautyFactor = 0.1f + Mathf.Pow(0.5f + 1f / (1f + Mathf.Pow(4f, -physicalFactor - personalityFactor + 0.12f)), 2.5f);



        /* PAWN SEX AND ROMANCE DRIVE FACTORS */
        float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
        float pawnRomanceDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
        float pawnDriveFactor = pawnRomanceDrive + 0.25f * pawnSexDrive;

        /*  MULTIPLY TO GET RESULT */
        __result = sexualityFactor * ageFactor * beautyFactor * pawnDriveFactor;
        Log.Message("SecondaryLovinChanceFactor between " + pawn.LabelShort + " and " + otherPawn.LabelShort + ", sexualityFactor = " + sexualityFactor + ", ageFactor = " + ageFactor + ", beautyFactor = " + beautyFactor + ", pawnDriveFactor = " + pawnDriveFactor + ", result = " + __result);
        return false;
    }

    public static float CalculateAgeFactor(Pawn pawn, Pawn otherPawn)
    {
        float age1 = pawn.ageTracker.AgeBiologicalYearsFloat;
        float age2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
        SpeciesSettings settings1 = PsychologySettings.speciesDict[pawn.def.defName];
        SpeciesSettings settings2 = PsychologySettings.speciesDict[otherPawn.def.defName];
        float minAge1 = settings1.minDatingAge;
        float minAge2 = PsychologySettings.speciesDict[otherPawn.def.defName].minDatingAge;
        bool pawnLecher = pawn.story.traits.HasTrait(TraitDefOfPsychology.Lecher);
        if (minAge1 == 0f || minAge2 == 0f)
        {
            return 1f;
        }
        if (minAge1 < 0f || age1 < minAge1)
        {
            return 0f;
        }
        if (minAge2 < 0f)
        {
            // Lechers are gross and will hit on aromantic species
            return pawnLecher ? 1f : 0f;
            //return 0f;
        }
        if (age2 < minAge2 && !pawnLecher)
        {
            // Lechers are gross and will hit on underage pawns
            return 0f;
        }
        float scaledAge1 = PsycheHelper.DatingAgeToVanilla(age1, minAge1);
        float scaledAge2 = PsycheHelper.DatingAgeToVanilla(age2, minAge2);
        float ageFactor = 1f;
        if (settings1.enableAgeGap && settings2.enableAgeGap)
        {
            float pawnOpenMinded = pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;
            float pawnExperimental = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
            float pawnPure = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
            //float minY = Mathf.Clamp01(0.2f + 0.8f * Mathf.Pow(pawnExperimental, 2) - 0.4f * pawnPure + 0.5f * pawnOpenMinded);
            float minY = Mathf.Clamp01(Mathf.Pow(0.5f * (pawnExperimental + 1f - pawnPure), 2f) + 0.5f * pawnOpenMinded);

            float pawnKinseyFactor = Mathf.InverseLerp(6f, 0f, PsycheHelper.Comp(pawn).Sexuality.kinseyRating);

            // Maybe one day other genders will come to the Rim...
            float pawnGenderFactor = pawn.gender == Gender.Female ? 1f : pawn.gender == Gender.Male ? -1f : 0f;

            float smallShift = 3.5f * pawnKinseyFactor * pawnGenderFactor;
            float largeShift = 10f * pawnKinseyFactor * pawnGenderFactor;

            List<float> offsets = new List<float>() { -20f + largeShift, -6.5f + smallShift, 6.5f + smallShift, 20f + largeShift };
            Log.Message("Age factor for pawn1 = " + pawn.LabelShort + ", pawn2 = " + otherPawn.LabelShort);
            ageFactor *= AgeGapFactor(scaledAge1, scaledAge2, minY, pawnLecher, offsets);
            ageFactor *= pawnLecher ? 1f : Mathf.InverseLerp(14f, 18f, scaledAge2);
        }
        return ageFactor;
    }

    public static float AgeGapFactor(float age1, float age2, float minY, bool lecher, List<float> offsets)
    {
        if (lecher)
        {
            if (age1 > age2)
            {
                // Gross
                return 1f;
            }
            minY = 0.5f * (minY + 1f);
        }
        float min = age1 + offsets[0];
        float lower = age1 + offsets[1];
        float upper = age1 + offsets[2];
        float max = age1 + offsets[3];
        float result = GenMath.FlatHill(minY, min, lower, upper, max, minY, age2);
        Log.Message("age1 = " + age1 + ", age2 = " + age2 + ", min = " + min + ", lower = " + lower + ", upper = " + upper + ", max = " + max + ", minY = " + minY + ", result = " + result);
        return result;
    }

}






//[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryRomanceChanceFactor))]
//public static class Pawn_RelationsTracker_RomanceChancePatch
//{
//    //[LogPerformance]
//    [HarmonyPostfix]
//    public static void PsychologyFormula(Pawn_RelationsTracker __instance, ref float __result, Pawn otherPawn)
//    {
//        Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
//        if (PsycheHelper.PsychologyEnabled(pawn))
//        {
//            /* Throw away the existing result and substitute our own formula. */
//            /* This formula is now used to determine dating chance. Loving frequency is determined by additional calculations  */
//            if (pawn.def != otherPawn.def || pawn == otherPawn || otherPawn.AnimalOrWildMan())
//            {
//                __result = 0f;
//                return;
//            }

//            /* SEXUAL PREFERENCE FACTOR */
//            float sexualityFactor = 1f;
//            /* Psychology result */
//            if (PsychologySettings.enableKinsey)
//            {
//                float kinsey = 3 - PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
//                float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
//                sexualityFactor = Mathf.InverseLerp(3f, 0f, kinsey * homo);
//            }
//            /* Vanilla result */
//            // Vanilla Asexual, Bisexual, and Gay traits
//            else if (pawn.story != null && pawn.story.traits != null)
//            {
//                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
//                {
//                    __result = 0f;
//                    return;
//                }
//                if (otherPawn.gender == pawn.gender)
//                {
//                    if (!pawn.story.traits.HasTrait(TraitDefOf.Gay) && !pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
//                    {
//                        __result = 0f;
//                        return;
//                    }
//                    // Vanilla: pawns are less likely to hit recipients who are the same gender and not Gay nor Bisexual
//                    if (!otherPawn.story.traits.HasTrait(TraitDefOf.Gay) && !otherPawn.story.traits.HasTrait(TraitDefOf.Bisexual))
//                    {
//                        sexualityFactor = 0.15f;
//                    }
//                }
//                else
//                {
//                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
//                    {
//                        __result = 0f;
//                        return;
//                    }
//                    // Vanilla: pawns are less likely to hit recipients who are not the same gender and Gay
//                    if (otherPawn.story.traits.HasTrait(TraitDefOf.Gay))
//                    {
//                        sexualityFactor = 0.15f;
//                    }
//                }
//            }


//            /* GET PAWN PERSONALITY VALUES */
//            //float pawnEmpathetic = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic);
//            float pawnExperimental = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
//            float pawnPure = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
//            float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
//            float pawnRomanceDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
//            float otherPawnCool = PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool);
//            float pawnOpenMinded = pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;
//            bool pawnLecher = pawn.story.traits.HasTrait(TraitDefOfPsychology.Lecher);

//            /* AGE FACTORS */
//            float ageFactor = 1f;
//            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
//            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
//            float minDatingAge1 = 13f;
//            float minDatingAge2 = ageBiologicalYearsFloat - Mathf.Max(1f, 4f + 6f * (pawnExperimental - pawnPure + pawnOpenMinded));
//            minDatingAge2 = Mathf.Clamp(minDatingAge2, minDatingAge1, 18f);
//            float maxDatingAge2 = ageBiologicalYearsFloat + Mathf.Max(1f, 4f + 6f * (pawnExperimental - pawnPure + pawnOpenMinded));
//            if (pawnLecher)
//            {
//                minDatingAge2 = minDatingAge1;
//                maxDatingAge2 = 10000f;
//            }
//            if (ageBiologicalYearsFloat < minDatingAge1 || ageBiologicalYearsFloat2 < minDatingAge2)
//            {
//                __result = 0f;
//                return;
//            }
//            if (ageBiologicalYearsFloat < 18f && ageBiologicalYearsFloat2 > maxDatingAge2)
//            {
//                __result = 0f;
//                return;
//            }
//            if (pawn.gender == Gender.Male)
//            {
//                float min = ageBiologicalYearsFloat - 30f;
//                float lower = ageBiologicalYearsFloat - 10f;
//                float upper = ageBiologicalYearsFloat + 3f;
//                float max = ageBiologicalYearsFloat + 10f;
//                float minY = Mathf.Clamp01(0.2f + 0.8f * Mathf.Pow(pawnExperimental, 2) - 0.4f * pawnPure + 0.5f * pawnOpenMinded);
//                float maxY = minY;
//                if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
//                {
//                    minY = 1f;
//                    maxY = 1f;
//                }
//                ageFactor = GenMath.FlatHill(minY, min, lower, upper, max, maxY, ageBiologicalYearsFloat2);
//            }
//            else if (pawn.gender == Gender.Female)
//            {
//                float min2 = ageBiologicalYearsFloat - 10f;
//                float lower2 = ageBiologicalYearsFloat - 3f;
//                float upper2 = ageBiologicalYearsFloat + 10f;
//                float max2 = ageBiologicalYearsFloat + 30f;
//                float minY2 = Mathf.Clamp01(0.2f + 0.8f * Mathf.Pow(pawnExperimental, 2) - 0.4f * pawnPure + 0.5f * pawnOpenMinded);
//                float maxY2 = minY2;
//                if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
//                {
//                    minY2 = 1f;
//                    maxY2 = 1f;
//                }
//                ageFactor = GenMath.FlatHill(minY2, min2, lower2, upper2, max2, maxY2, ageBiologicalYearsFloat2);
//            }

//            /* BEAUTY FACTOR */
//            float pawnBeauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
//            float otherPawnBeauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
//            otherPawnBeauty += 2f * otherPawnCool - 1f;
//            /* Beautiful pawns will have higher beauty standards. Everyone wants to date out of league */
//            float beautyFactor = otherPawnBeauty - 0.75f * pawnBeauty;
//            //if (beautyFactor > 0)
//            //{
//            //    /* Cool Pawns enhance their beauty */
//            //    beautyFactor *= 0.67f * (1f + otherPawnCool);
//            //}
//            //else
//            //{
//            //    /* Empathetic pawns don't care as much about negative beauty */
//            //    beautyFactor *= 1.5f - pawnEmpathetic;
//            //}
//            /* Cool pawns are more attractive */
//            beautyFactor += 2f * otherPawnCool - 1f;
//            /* Open Minded pawns don't care about beauty */
//            beautyFactor *= 1f - pawnOpenMinded;
//            /* Pawns who can't see as well can't determine beauty as well. */
//            beautyFactor *= 0.1f + 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
//            /* Turn into multiplicative factor */
//            beautyFactor = Mathf.Pow(0.5f + 1f / (1f + Mathf.Pow(6f, -beautyFactor)), 2f);

//            /* PAWN SEX AND ROMANCE DRIVE FACTORS */
//            // Should this go into MtbLovingHours instead?
//            float pawnDriveFactor = 0.5f * Mathf.Pow(pawnSexDrive, 2) + 0.5f * pawnRomanceDrive;

//            /* DISABILITY FACTOR */
//            // Should this go into MtbLovingHours instead?
//            //float disabilityFactor = 0f;
//            //disabilityFactor += Mathf.Lerp(0f, -2f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
//            //disabilityFactor += Mathf.Lerp(0f, -2f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
//            //disabilityFactor += Mathf.Lerp(0f, -2f, otherPawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
//            ///* More experimental pawns won't care as much about disabilities */
//            //disabilityFactor = Mathf.Lerp(disabilityFactor, 0f, PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental));
//            ///* Open minded pawns won't care as much about disabilities */
//            //disabilityFactor *= 1f - 0.9f * pawnOpenMinded;
//            ///* Turn into multiplicative factor */
//            //disabilityFactor = Mathf.Pow(2, disabilityFactor);


//            /*  MULTIPLY TO GET RESULT */
//            __result = sexualityFactor * ageFactor * beautyFactor * pawnDriveFactor;
//        }
//    }
//}

//[HarmonyPostfix]
//public static void PsychologyFormula(Pawn_RelationsTracker __instance, ref float __result, Pawn ___pawn, Pawn otherPawn)
//{
//    //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
//    Pawn pawn = ___pawn;
//    if (!PsycheHelper.PsychologyEnabled(pawn) || __result == 0f)
//    {
//        return;
//    }

//    /* SEXUAL PREFERENCE FACTOR */
//    if (PsychologySettings.enableKinsey)
//    {
//        float kinsey = 3 - PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
//        float homo = (pawn.gender == otherPawn.gender) ? 1f : -1f;
//        __result *= Mathf.InverseLerp(3f, 0f, kinsey * homo);
//    }

//    /* GET PAWN PERSONALITY VALUES */
//    float pawnAggressive = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
//    float pawnConfident = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Confident);
//    float pawnJudgmental = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental);
//    float pawnExperimental = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
//    float pawnPure = PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
//    float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
//    float pawnRomanceDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedRomanticDrive;
//    float otherPawnCool = PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool);
//    float pawnOpenMinded = pawn.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;
//    bool pawnLecher = pawn.story.traits.HasTrait(TraitDefOfPsychology.Lecher);

//    /* AGE FACTORS */
//    float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
//    float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
//    //float minDatingAge1 = 13f;
//    //float minDatingAge2 = ageBiologicalYearsFloat - Mathf.Max(1f, 4f + 6f * (pawnExperimental - pawnPure + pawnOpenMinded));
//    //minDatingAge2 = Mathf.Clamp(minDatingAge2, minDatingAge1, 18f);
//    //float maxDatingAge2 = ageBiologicalYearsFloat + Mathf.Max(1f, 4f + 6f * (pawnExperimental - pawnPure + pawnOpenMinded));
//    //if (pawnLecher)
//    //{
//    //    minDatingAge2 = minDatingAge1;
//    //    maxDatingAge2 = 10000f;
//    //}
//    //if (ageBiologicalYearsFloat < minDatingAge1 || ageBiologicalYearsFloat2 < minDatingAge2)
//    //{
//    //    __result = 0f;
//    //    return;
//    //}
//    //if (ageBiologicalYearsFloat < 18f && ageBiologicalYearsFloat2 > maxDatingAge2)
//    //{
//    //    __result = 0f;
//    //    return;
//    //}
//    float min = 16f;
//    float lower = 18f;
//    float upper = 1e+3f;
//    float max = 1e+4f;
//    // if (ModIsActive("Androids") ...
//    if (pawn.gender == Gender.Male)
//    {
//        min = ageBiologicalYearsFloat - 30f;
//        lower = ageBiologicalYearsFloat - 10f;
//        upper = ageBiologicalYearsFloat + 3f;
//        max = ageBiologicalYearsFloat + 10f;
//        // Undo Vanilla result
//        __result /= GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, ageBiologicalYearsFloat2);
//    }
//    else if (pawn.gender == Gender.Female)
//    {
//        min = ageBiologicalYearsFloat - 10f;
//        lower = ageBiologicalYearsFloat - 3f;
//        upper = ageBiologicalYearsFloat + 10f;
//        max = ageBiologicalYearsFloat + 30f;
//        // Undo Vanilla result
//        __result /= GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, ageBiologicalYearsFloat2);
//    }
//    // Psychology result
//    float minY = pawnLecher ? 1f : Mathf.Clamp01(0.2f + 0.8f * Mathf.Pow(pawnExperimental, 2) - 0.4f * pawnPure + 0.5f * pawnOpenMinded);
//    __result *= GenMath.FlatHill(minY, min, lower, upper, max, minY, ageBiologicalYearsFloat2);

//    // Romance and sex drive factors already account for age
//    __result /= Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat);
//    __result /= Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat);
//    /* PAWN SEX AND ROMANCE DRIVE FACTORS */
//    __result *= 0.01f + 1.25f * Mathf.Pow(pawnRomanceDrive, 2) + 0.25f * pawnSexDrive;

//    /* BEAUTY FACTOR */
//    float pawnBeauty = 0f;
//    if (otherPawn.RaceProps.Humanlike)
//    {
//        pawnBeauty = pawn.GetStatValue(StatDefOf.PawnBeauty);
//    }
//    float otherPawnBeauty = 0f;
//    if (otherPawn.RaceProps.Humanlike)
//    {
//        otherPawnBeauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
//    }
//    // Undo vanilla factor
//    __result /= otherPawnBeauty < 0f ? 0.3f : otherPawnBeauty > 0f ? 2.3f : 1f;
//    /* Beautiful pawns will have higher beauty standards. Everyone wants to date out of league */
//    float beautyAdditiveFactor = otherPawnBeauty - 0.75f * pawnBeauty;
//    /* Cool pawns are more attractive */
//    beautyAdditiveFactor += otherPawnCool - 0.5f;
//    /* Open Minded pawns don't care about beauty */
//    beautyAdditiveFactor *= 1f - pawnOpenMinded;
//    /* Pawns who can't see as well can't determine beauty as well. */
//    beautyAdditiveFactor *= 0.1f + 0.9f * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
//    /* Judgmental pawns will care more either way */
//    beautyAdditiveFactor *= 2f * pawnJudgmental;
//    /* Turn into multiplicative factor */
//    __result *= Mathf.Pow(2f, beautyAdditiveFactor);
//}