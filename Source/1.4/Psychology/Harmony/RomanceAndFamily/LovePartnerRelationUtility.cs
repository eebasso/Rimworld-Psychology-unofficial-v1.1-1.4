using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.LovePartnerRelationGenerationChance))]
public static class LovePartnerRelationUtility_GenerationChancePatch
{
  [HarmonyPrefix]
  public static bool LovePartnerRelationGenerationChance(ref float __result, Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
  {
    if (!SpeciesHelper.RomanceEnabled(generated, true) || !SpeciesHelper.RomanceEnabled(other, true))
    {
      __result = 0f;
      return false;
    }

    bool sameGender = generated.gender == other.gender;

    float sexualityFactor = 1f;
    //if (PsychologySettings.enableKinsey)
    if (PsychologySettings.enableKinsey && PsycheHelper.PsychologyEnabled(generated) && PsycheHelper.PsychologyEnabled(other))
    {
      int kinsey1 = PsycheHelper.Comp(generated).Sexuality.kinseyRating;
      int kinsey2 = PsycheHelper.Comp(other).Sexuality.kinseyRating;
      sexualityFactor *= Mathf.Clamp01(sameGender ? kinsey1 / 3f : 2f - kinsey1 / 3f);
      sexualityFactor *= Mathf.Clamp01(sameGender ? kinsey2 / 3f : 2f - kinsey2 / 3f);
    }
    else
    {
      if (sameGender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !request.AllowGay))
      {
        __result = 0f;
        return false;
      }
      if (!sameGender && other.story.traits.HasTrait(TraitDefOf.Gay))
      {
        __result = 0f;
        return false;
      }
      sexualityFactor = sameGender ? 0.01f : 1f;
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
    SpeciesSettings generatedSettings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(generated.def);
    SpeciesSettings otherSettings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(other.def);

    float bioAge1 = generated.ageTracker.AgeBiologicalYearsFloat;
    float bioAge2 = other.ageTracker.AgeBiologicalYearsFloat;
    float minDatingAge1 = generatedSettings.minDatingAge;
    float minDatingAge2 = otherSettings.minDatingAge;

    bool minAge1IsNotZero = minDatingAge1 != 0f;
    bool minAge2IsNotZero = minDatingAge2 != 0f;
    float generationChanceAgeFactor1 = 1f;
    float generationChanceAgeFactor2 = 1f;
    float generationChanceAgeGapFactor = 1f;
    float scaledBioAge1 = 0f;
    float scaledBioAge2 = 0f;
    if (minAge1IsNotZero)
    {
      scaledBioAge1 = PsycheHelper.DatingBioAgeToVanilla(bioAge1, minDatingAge1);
      generationChanceAgeFactor1 = GetGenerationChanceAgeFactor(scaledBioAge1);
    }
    if (minAge2IsNotZero)
    {
      scaledBioAge2 = PsycheHelper.DatingBioAgeToVanilla(bioAge2, minDatingAge2);
      generationChanceAgeFactor2 = GetGenerationChanceAgeFactor(scaledBioAge2);
    }
    if (minAge1IsNotZero && minAge2IsNotZero && generatedSettings.enableAgeGap && otherSettings.enableAgeGap)
    {
      float scaledChrAge1 = PsycheHelper.DatingBioAgeToVanilla(generated.ageTracker.AgeChronologicalYearsFloat, minDatingAge1);
      float scaledChrAge2 = PsycheHelper.DatingBioAgeToVanilla(other.ageTracker.AgeChronologicalYearsFloat, minDatingAge2);
      generationChanceAgeGapFactor = GetGenerationChanceAgeGapFactor(scaledBioAge1, scaledBioAge2, scaledChrAge1, scaledChrAge2, ex);
    }
    float incestFactor = 1f;
    if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
    {
      incestFactor = 0.01f;
    }
    __result = existingExLoverFactor * sexualityFactor * generationChanceAgeFactor1 * generationChanceAgeFactor2 * generationChanceAgeGapFactor * incestFactor;// * melaninFactor;
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
    float num2 = PawnRelationUtility.MaxPossibleBioAgeAt(scaledBioAge2, scaledChrAge2, num);
    float num3 = PawnRelationUtility.MinPossibleBioAgeAt(scaledBioAge2, num);
    //if (num2 < 0f)
    //{
    //    return -1f;
    //}
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

  // The maximum possible bio age at time atChronologicalAge
  //public static float MaxPossibleBioAgeAt(float myBiologicalAge, float myChronologicalAge, float atChronologicalAge)
  //{
  //    // atChronologicalAge is atChrAge - 14
  //    // myChronologicalAge - atChronologicalAge
  //    float num = Mathf.Min(myBiologicalAge, myChronologicalAge - atChronologicalAge);
  //    if (num < 0f)
  //    {
  //        return -1f;
  //    }
  //    return num;
  //}

  //public static float MinPossibleBioAgeAt(float myBiologicalAge, float atChronologicalAge)
  //{
  //    return Mathf.Max(myBiologicalAge - atChronologicalAge, 0f);
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

[HarmonyPatch(typeof(LovePartnerRelationUtility), "LovinMtbSinglePawnFactor")]
public static class LovePartnerRelationUtility_LovinMtbSinglePawnFactor_Patch
{
  [HarmonyPostfix]
  public static void LovinMtbSinglePawnFactorPostfix(ref float __result, Pawn pawn)
  {
    float mtbHours = SexualityHelper.LovinMtbHoursPsychology(pawn);
    // The factor of 12f here cancels out the 12f appearing in LovePartnerRelationUtility.GetLovinMtbHours
    __result = mtbHours < 0f ? -1f : Mathf.Sqrt(0.5f * mtbHours / 12f);
  }
}

//[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
//public static class LovePartnerRelationUtility_GetLovinMtbHours
//{
//  [HarmonyTranspiler]
//  public static IEnumerable<CodeInstruction> GetLovinMtbHoursTranspiler(IEnumerable<CodeInstruction> codes)
//  {
//    //Log.Message("GetLovinMtbHoursTranspiler, start");
//    foreach (CodeInstruction c in codes)
//    {
//      if (c.OperandIs(12f))
//      {
//        //Log.Message("GetLovinMtbHoursTranspiler, found code");
//        c.opcode = OpCodes.Ldarg_0;
//        c.operand = null;
//        yield return c;
//        yield return new CodeInstruction(OpCodes.Ldarg_1);
//        //Log.Message("GetLovinMtbHoursTranspiler, start invoke");
//        yield return CodeInstruction.Call(typeof(PsychologyRomancePatchUtility), nameof(PsychologyRomancePatchUtility.NewLovinMtbHoursScale), new Type[] { typeof(Pawn), typeof(Pawn) });
//        //Log.Message("GetLovinMtbHoursTranspiler, done invoke");
//      }
//      else
//      {
//        yield return c;
//      }
//    }
//    //Log.Message("GetLovinMtbHoursTranspiler, end");
//  }
//}

//[HarmonyPrefix]
//public static bool GetLovinMtbHoursPrefix(ref float __result, Pawn pawn, Pawn partner)
//{
//    if (!PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(partner))
//    {
//        __result = -1f;
//        return false;
//    }
//    if (!SpeciesHelper.RomanceCombinedAgeCheck(pawn, false) || !SpeciesHelper.RomanceCombinedAgeCheck(partner, false))
//    {
//        ////Log.Message("GetLovinMtbHours, RomanceLifestageCheck != true");
//        // No underage lovin
//        __result = -1f;
//        return false;
//    }
//    if (pawn?.ageTracker?.Adult != true || partner?.ageTracker?.Adult != true)
//    {
//        //long AdultMinAgeTicks = pawn.ageTracker.AdultMinAgeTicks;
//        //long AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
//        //long TicksToAdulthood = AdultMinAgeTicks - AgeBiologicalTicks;
//        //bool Adult = TicksToAdulthood <= 0f;
//        //long AdultMinAgeTicks2 = partner.ageTracker.AdultMinAgeTicks;
//        //long AgeBiologicalTicks2 = partner.ageTracker.AgeBiologicalTicks;
//        //long TicksToAdulthood2 = AdultMinAgeTicks2 - AgeBiologicalTicks2;
//        //bool Adult2 = TicksToAdulthood2 <= 0f;
//        ////Log.Message("GetLovinMtbHours, pawn: " + pawn + ", AdultMinAgeTicks: " + AdultMinAgeTicks + ", AgeBiologicalTicks: " + AgeBiologicalTicks + ", TicksToAdulthood: " + TicksToAdulthood + ", Adult: " + Adult + "\nGetLovinMtbHours, pawn2: " + partner + ", AdultMinAgeTicks2: " + AdultMinAgeTicks2 + ", AgeBiologicalTicks2: " + AgeBiologicalTicks2 + ", TicksToAdulthood2: " + TicksToAdulthood2 + ", Adult2: " + Adult2);

//        // No underage lovin
//        __result = -1f;
//        return false;
//    }
//    return true;
//}

//[HarmonyPrefix]
//public static bool GetLovinMtbHours(ref float __result, Pawn pawn, Pawn partner)
//{
//    if (!PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(partner))
//    {
//        __result = -1;
//        return false;
//        //return true;
//    }
//    if (pawn.Dead || partner.Dead)
//    {
//        __result = -1f;
//        return false;
//    }
//    if (DebugSettings.alwaysDoLovin)
//    {
//        __result = 0.1f;
//        return false;
//    }
//    if (pawn.needs.food.Starving || partner.needs.food.Starving)
//    {
//        //Log.Message("GetLovinMtbHours, pawn.needs.food.Starving == true");
//        __result = -1f;
//        return false;
//    }
//    if (pawn.health.hediffSet.BleedRateTotal > 0f || partner.health.hediffSet.BleedRateTotal > 0f)
//    {
//        //Log.Message("GetLovinMtbHours, BleedRateTotal > 0f");
//        __result = -1f;
//        return false;
//    }
//    if (!pawn?.ageTracker?.Adult != true || !partner?.ageTracker?.Adult != true)
//    {
//        //Log.Message("GetLovinMtbHours, pawn?.ageTracker?.Adult != true");
//        // No underage lovin
//        __result = -1f;
//        return false;
//    }
//    if (!SpeciesHelper.RomanceLifestageCheck(pawn, false) || !SpeciesHelper.RomanceLifestageCheck(partner, false))
//    {
//        //Log.Message("GetLovinMtbHours, RomanceLifestageCheck != true");
//        // No underage lovin
//        __result = -1f;
//        return false;
//    }

//    //float pawnAge = pawn.ageTracker.AgeBiologicalYearsFloat;
//    //float partnerAge = partner.ageTracker.AgeBiologicalYearsFloat;
//    //SpeciesSettings pawnSpeciesSettings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
//    //SpeciesSettings partnerSpeciesSettings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(partner.def);

//    //float pawnMinLovinAge = PsychologySettings.speciesDict[pawn.def.defName].minLovinAge;
//    //float partnerMinLovinAge = PsychologySettings.speciesDict[partner.def.defName].minLovinAge;
//    //if (pawnAge < pawnMinLovinAge || partnerAge < partnerMinLovinAge || pawnMinLovinAge < 0f || partnerMinLovinAge < 0f)
//    //{
//    //    // No underage lovin
//    //    __result = -1f;
//    //    return false;
//    //}

//    float pawnSexDrive = PsycheHelper.Comp(pawn).Sexuality.AdjustedSexDrive;
//    float partnerSexDrive = PsycheHelper.Comp(partner).Sexuality.AdjustedSexDrive;
//    if (pawnSexDrive < 0.1f || partnerSexDrive < 0.1f)
//    {
//        //Log.Message("GetLovinMtbHours, sex drive too low");
//        // Asexual pawns avoid lovin
//        __result = -1f;
//        return false;
//    }
//    float frequency = 1f;
//    frequency *= LovinMtbSinglePawnFactor(pawn);
//    frequency *= LovinMtbSinglePawnFactor(partner);
//    frequency *= Mathf.Pow(pawnSexDrive, 2f);
//    frequency *= Mathf.Pow(partnerSexDrive, 2f);
//    frequency *= Mathf.Max(pawn.relations.SecondaryLovinChanceFactor(partner), 0.1f);
//    frequency *= Mathf.Max(partner.relations.SecondaryLovinChanceFactor(pawn), 0.1f);
//    frequency /= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, pawn.relations.OpinionOf(partner));
//    frequency /= GenMath.LerpDouble(-100f, 100f, 1.3f, 0.7f, partner.relations.OpinionOf(pawn));
//    if (pawn.health.hediffSet.HasHediff(HediffDefOf.PsychicLove))
//    {
//        frequency *= 4f;
//    }
//    __result = frequency > 0f ? 12f / frequency : -1f;
//    return false;
//}

//public static float LovinMtbSinglePawnFactor(Pawn pawn)
//{
//    float num = 1f;
//    num *= 1f - pawn.health.hediffSet.PainTotal;
//    float level = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness);
//    if (level < 0.5f)
//    {
//        num *= level * 2f;
//    }
//    return num;
//}