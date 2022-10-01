using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Linq;using System.Reflection.Emit;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(PawnRelationWorker_Sibling), "GenerateParentParams")]
public static class PawnRelationWorker_Sibling_GenerateParentParams_Patch
{
    [HarmonyPrefix]
    public static bool GenerateParentParams(ref float minChronologicalAge, ref float maxChronologicalAge, ref float midChronologicalAge, ref float minBioAgeToHaveChildren, Pawn generatedChild, Pawn existingChild)
    {
        float chrAge1 = generatedChild.ageTracker.AgeChronologicalYearsFloat;
        float chrAge2 = existingChild.ageTracker.AgeChronologicalYearsFloat;

        //float num4 = minChronologicalAge;

        SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(existingChild.def);
        float minLovingAge = settings.minLovinAge;
        if (minLovingAge < 0f)
        {
            minLovingAge = 0f;
        }
        if (minLovingAge == 0f || !settings.enableAgeGap)
        {
            //float maxBioAge = existingChild.RaceProps.lifeExpectancy;
            if (existingChild.RaceProps?.lifeExpectancy is float maxBioAge != true)
            {
                maxBioAge = 80f;
            }
            minChronologicalAge = chrAge1 + minLovingAge;
            midChronologicalAge = chrAge1 + 0.5f * (maxBioAge + minLovingAge);
            maxChronologicalAge = chrAge1 + maxBioAge;
            minBioAgeToHaveChildren = minLovingAge;
            return true;
        }
        float chrAgeMax = Mathf.Max(chrAge1, chrAge2);
        float num = minChronologicalAge - chrAgeMax;
        float num2 = maxChronologicalAge - chrAgeMax;
        float num3 = midChronologicalAge - chrAgeMax;

        // Convert from hardcoded vanilla values
        num = PsycheHelper.LovinAgeFromVanilla(num, minLovingAge);
        num2 = PsycheHelper.LovinAgeFromVanilla(num2, minLovingAge);
        num3 = PsycheHelper.LovinAgeFromVanilla(num3, minLovingAge);

        // Recompute
        minChronologicalAge = chrAgeMax + num;
        maxChronologicalAge = chrAgeMax + num2;
        midChronologicalAge = chrAgeMax + num3;
        minBioAgeToHaveChildren = num;
        return true;

        //SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(existingChild.def);
        //float minLovingAge = settings.minLovinAge;
        //float childChrAge = Mathf.Max(generatedChild.ageTracker.AgeChronologicalYearsFloat, existingChild.ageTracker.AgeChronologicalYearsFloat);
        //if (minLovingAge <= 0f || !settings.enableAgeGap)
        //{
        //    float maxBioAge = existingChild.RaceProps.lifeExpectancy;
        //    minChronologicalAge = childChrAge + minLovingAge;
        //    midChronologicalAge = childChrAge + 0.5f * (maxBioAge + minLovingAge);
        //    maxChronologicalAge = childChrAge + maxBioAge;
        //    minBioAgeToHaveChildren = minLovingAge;
        //    return true;
        //}
        //float minVanillaAge = minChronologicalAge - childChrAge;
        //float midVanillaAge = midChronologicalAge - childChrAge;
        //float maxVanillaAge = maxChronologicalAge - childChrAge;

        //minBioAgeToHaveChildren = PsycheHelper.LovinAgeFromVanilla(minVanillaAge, minLovingAge);
        //minChronologicalAge = childChrAge + minBioAgeToHaveChildren;
        //midChronologicalAge = childChrAge + PsycheHelper.LovinAgeFromVanilla(midVanillaAge, minLovingAge);
        //maxChronologicalAge = childChrAge + PsycheHelper.LovinAgeFromVanilla(maxVanillaAge, minLovingAge);
        //return true;
    }
}

public static class PawnRelationWorker_Sibling_ManualPatches
{
    public static IEnumerable<CodeInstruction> CreateRelations_Transpiler(IEnumerable<CodeInstruction> codes)
    {
        Log.Message("CreateRelations_Transpiler, start");
        List<CodeInstruction> clist = codes.ToList();
        int max = clist.Count();
        bool bool1;
        bool bool2;

        Log.Message("CreateRelations_Transpiler, start while");
        for (int i = 0; i < max; i++)
        {
            yield return clist[i];
            if (i >= max - 4)
            {
                continue;
            }
            bool1 = clist[i + 3].LoadsField(AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Gay)));
            bool2 = clist[i + 4].Calls(AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef) }));
            if (!bool1 || !bool2)
            {
                continue;
            }
            Log.Message("bools satisfied");
            yield return CodeInstruction.Call(typeof(PawnRelationWorker_Sibling_ManualPatches), nameof(DivorceBasedOnSexuality), new Type[] { typeof(Pawn) });
            Log.Message("call satisfied");
            i += 4;
        }
        Log.Message("CreateRelations_Transpiler, end");
    }

    public static bool DivorceBasedOnSexuality(Pawn parent)
    {
        bool flag = true;
        if (PsycheHelper.TryGetPawnSeed(parent) != true)
        {
            flag = PsycheHelper.HasTraitDef(parent, TraitDefOf.Gay);
            if (flag)
            {
                Log.Error("CreateRelations.DivorceBasedOnSexuality, TryGetPawnSeed(parent) != true but pawn has TraitDefOf.Gay");
            }
            return flag;
        }
        if (PsycheHelper.PsychologyEnabled(parent) != true)
        {
            flag = PsycheHelper.HasTraitDef(parent, TraitDefOf.Gay);
            if (flag)
            {
                Log.Error("CreateRelations.DivorceBasedOnSexuality, PsychologyEnabled(parent) != true but pawn has TraitDefOf.Gay");
            }
            return flag;
        }

        int kinsey = PsycheHelper.Comp(parent).Sexuality.kinseyRating;
        flag = kinsey > 4;
        Log.Message("DivorceBasedOnSexuality fired, flag = " + flag);

        //return kinsey > 5;
        return flag;
    }
}

//[HarmonyPatch(typeof(PawnRelationWorker_Sibling), "GenerateParent")]
//public static class PawnRelationWorker_Sibling_GenerateParent_Patch
//{

//[HarmonyPrefix]
//public static bool KinseyException(ref Pawn __result, Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
//{
//    if (PsychologySettings.enableKinsey)
//    {
//        //TODO: Turn this into a transpiler instead of a prefix.
//        float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
//        float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
//        float num = (genderToGenerate != Gender.Male) ? 16f : 14f;
//        float num2 = (genderToGenerate != Gender.Male) ? 45f : 50f;
//        float num3 = (genderToGenerate != Gender.Male) ? 27f : 30f;
//        float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
//        float maxChronologicalAge = num4 + (num2 - num);
//        float midChronologicalAge = num4 + (num3 - num);
//        var parameters = new object[] { num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, null, null, null, null };
//        Traverse.Create(typeof(PawnRelationWorker_Sibling)).Method("GenerateParentParams", new Type[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(Pawn), typeof(Pawn), typeof(PawnGenerationRequest), typeof(float).MakeByRefType(), typeof(float).MakeByRefType(), typeof(float).MakeByRefType(), typeof(string).MakeByRefType() }).GetValue(parameters);
//        float value = (float)parameters[7];
//        float value2 = (float)parameters[8];
//        float value3 = (float)parameters[9];
//        string last = (string)parameters[10];
//        bool allowGay = true;
//        Pawn parent = null;
//        if (genderToGenerate == Gender.Male && existingChild.GetMother() != null)
//        {
//            parent = existingChild.GetMother();
//        }
//        else if (genderToGenerate == Gender.Female && existingChild.GetFather() != null)
//        {
//            parent = existingChild.GetFather();
//        }
//        if (PsycheHelper.PsychologyEnabled(parent))
//        {
//            float kinsey = 3 - PsycheHelper.Comp(parent).Sexuality.kinseyRating;
//            float num5 = Mathf.InverseLerp(3f, 0f, -kinsey);
//            if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < num5)
//            {
//                last = ((NameTriple)parent.Name).Last;
//                allowGay = false;
//            }
//        }
//        Faction faction = existingChild.Faction;
//        if (faction == null || faction.IsPlayer)
//        {
//            bool tryMedievalOrBetter = faction != null && faction.def.techLevel >= TechLevel.Medieval;
//            if (!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter, true, TechLevel.Undefined))
//            {
//                faction = Faction.OfAncients;
//            }
//        }
//        PawnKindDef kindDef = existingChild.kindDef;
//        Faction faction2 = faction;
//        bool forceGenerateNewPawn = true;
//        bool allowDead = true;
//        bool allowDowned = true;
//        bool canGeneratePawnRelations = false;
//        Gender? fixedGender = new Gender?(genderToGenerate);
//        float? fixedMelanin = new float?(value3);
//        string fixedLastName = last;
//        //Gender? fixedGender = null, float? fixedMelanin = null, string fixedLastName = null, string fixedBirthName = null, RoyalTitleDef fixedTitle = null, Ideo fixedIdeo = null, bool forceNoIdeo = false, bool forceNoBackstory = false, bool forbidAnyTitle = false);
//        PawnGenerationRequest request = new PawnGenerationRequest(
//            kindDef,                            // PawnKindDef kind
//            faction2,                           // Faction faction = null
//            PawnGenerationContext.NonPlayer,    // PawnGenerationContext context = PawnGenerationContext.NonPlayer  
//            -1,                                 // int tile = -1
//            forceGenerateNewPawn,               // bool forceGenerateNewPawn = false
//            false,                              // bool newborn = false
//            allowDead,                          // bool allowDead = false
//            allowDowned,                        // bool allowDowned = false
//            canGeneratePawnRelations,           // bool canGeneratePawnRelations = true
//            false,                              // bool mustBeCapableOfViolence = false
//            1f,                                 // float colonistRelationChanceFactor = 1
//            false,                              // bool forceAddFreeWarmLayerIfNeeded = false
//            allowGay,                           // bool allowGay = true
//            true,                               // bool allowFood = true
//            false,                              // bool allowAddictions = true
//            false,                              // bool inhabitant = false
//            false,                              // bool certainlyBeenInCryptosleep = false
//            false,                              // bool forceRedressWorldPawnIfFormerColonist = false
//            false,                              // bool worldPawnFactionDoesntMatter = false
//            0f,                                 // float biocodeWeaponChance = 0
//            0f,                               // float biocodeApparelChance = 0
//            null,                                 // Pawn extraPawnForExtraRelationChance = null
//            1f,                               // float relationWithExtraPawnChanceFactor = 1
//            null,                               // Predicate<Pawn> validatorPreGear = null
//            null,                               // Predicate<Pawn> validatorPostGear = null
//            null,                               // IEnumerable<TraitDef> forcedTraits = null
//            null,                               // IEnumerable<TraitDef> prohibitedTraits = null
//            new float?(value),                  // float? minChanceToRedressWorldPawn = null
//            new float?(value2),                 // float? fixedBiologicalAge = null
//            null,                               // float? fixedChronologicalAge = null
//            fixedGender,                       // Gender? fixedGender = null
//            fixedMelanin,                       // float? fixedMelanin = null
//            fixedLastName                                    // string fixedLastName = null
//        );
//        Pawn pawn = PawnGenerator.GeneratePawn(request);
//        if (!Find.WorldPawns.Contains(pawn))
//        {
//            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
//        }
//        __result = pawn;
//        return false;
//    }
//    return true;
//}
//}

