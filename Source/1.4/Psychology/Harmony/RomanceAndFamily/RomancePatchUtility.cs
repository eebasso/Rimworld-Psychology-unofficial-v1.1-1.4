using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;
using System.Xml;
using System.Security.Cryptography;

namespace Psychology.Harmony;

public class RomancePatchUtility
{
  public static Pawn cachedPawn;

  public static IEnumerable<CodeInstruction> ChangeMinRomanceOpinion(IEnumerable<CodeInstruction> codes)
  {
    foreach (CodeInstruction c in codes)
    {
      if (c.OperandIs(5f))
      {
        yield return CodeInstruction.LoadField(typeof(PsychologySettings), nameof(PsychologySettings.romanceOpinionThreshold));
        continue;
      }
      yield return c;
    }
  }

  public static IEnumerable<CodeInstruction> ChangeLogicForSexualityTraits(IEnumerable<CodeInstruction> codes, string failureMessage)
  {
    FieldInfo storyFieldInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.story));
    FieldInfo traitsFieldInfo = AccessTools.Field(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.traits));
    MethodInfo hasTraitMethodInfo1 = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef) });
    MethodInfo hasTraitMethodInfo2 = AccessTools.Method(typeof(TraitSet), nameof(TraitSet.HasTrait), new Type[] { typeof(TraitDef), typeof(int) });

    FieldInfo asexualFieldInfo = AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Asexual));
    FieldInfo bisexualFieldInfo = AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Bisexual));
    FieldInfo gayFieldInfo = AccessTools.Field(typeof(TraitDefOf), nameof(TraitDefOf.Gay));

    int numMatches = 0;

    List<CodeInstruction> clist = codes.ToList();

    for (int i = 0; i < clist.Count; i++)
    {
      yield return clist[i];
      if (i >= clist.Count - 4)
      {
        continue;
      }
      if (clist[i + 1].LoadsField(storyFieldInfo) != true)
      {
        continue;
      }
      if (clist[i + 2].LoadsField(traitsFieldInfo) != true)
      {
        continue;
      }
      int index = Math.Min(i + 5, clist.Count - 1);
      int hasTraitIndex = clist[i + 4].Calls(hasTraitMethodInfo1) ? i + 4 : clist[index].Calls(hasTraitMethodInfo2) ? i + 5 : -1;
      if (hasTraitIndex == -1)
      {
        continue;
      }
      if (clist[i + 3].LoadsField(asexualFieldInfo))
      {
        yield return CodeInstruction.Call(typeof(RomancePatchUtility), nameof(AsexualCheckKinseyEnabled));
      }
      else if (clist[i + 3].LoadsField(bisexualFieldInfo))
      {
        yield return CodeInstruction.Call(typeof(RomancePatchUtility), nameof(BisexualCheckKinseyEnabled));
      }
      else if (clist[i + 3].LoadsField(gayFieldInfo))
      {
        yield return CodeInstruction.Call(typeof(RomancePatchUtility), nameof(GayCheckKinseyEnabled));
      }
      else
      {
        continue;
      }
      numMatches++;
      i = hasTraitIndex;
    }
    if (numMatches == 0)
    {
      Log.Error(failureMessage);
      yield break;
    }
    ////Log.Message("ChangeLogicForSexualityTraits, found number of matches = " + numMatches);
  }

  public static bool AsexualCheckKinseyEnabled(Pawn pawn) => PsycheHelper.PsychologyEnabled(pawn) ? PsycheHelper.Comp(pawn).Sexuality.IsAsexual : false;

  public static bool BisexualCheckKinseyEnabled(Pawn pawn) => PsycheHelper.PsychologyEnabled(pawn) ? PsycheHelper.Comp(pawn).Sexuality.IsAnyAmountOfBisexual : false;

  public static bool GayCheckKinseyEnabled(Pawn pawn) => PsycheHelper.PsychologyEnabled(pawn) ? PsycheHelper.Comp(pawn).Sexuality.IsCompletelyGay : false;

  public static IEnumerable<CodeInstruction> InterdictMinRomanceAgeChecks(IEnumerable<CodeInstruction> codes)
  {
    //Log.Message("InterdictRomanceAges, start");
    bool failure = true;
    FieldInfo ageTrackerFieldInfo = AccessTools.Field(typeof(Pawn), nameof(Pawn.ageTracker));
    if (ageTrackerFieldInfo == null)
    {
      Log.Error("InterdictRomanceAges, ageTrackerFieldInfo == null");
    }
    MethodInfo ageBioYearsFloatGetter = AccessTools.PropertyGetter(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.AgeBiologicalYearsFloat));
    if (ageBioYearsFloatGetter == null)
    {
      Log.Error("InterdictRomanceAges, ageBioYearsFloatFieldInfo == null");
    }
    List<CodeInstruction> c = codes.ToList();

    for (int i = 0; i < c.Count; i++)
    {
      yield return c[i];
      if (i < c.Count - 3)
      {
        bool bool0 = c[i + 1].LoadsField(ageTrackerFieldInfo);
        bool bool1 = c[i + 2].Calls(ageBioYearsFloatGetter);
        string methodName = RomanceAgeCheckCorrectMethodName(c[i + 3]);
        if (bool0 && bool1 && methodName != null)
        {
          Log.Message("Found loads, calls, and 14 or 16");
          failure = false;
          yield return CodeInstruction.Call(typeof(RomancePatchUtility), nameof(GetAgeAndStorePawn));
          yield return CodeInstruction.Call(typeof(RomancePatchUtility), methodName);
          i += 3;
        }
      }
    }
    if (failure)
    {
      Log.Error("Psychology: patch with InterdictRomanceAges failed.");
    }
    //Log.Message("InterdictRomanceAges, end");
  }

  public static string RomanceAgeCheckCorrectMethodName(CodeInstruction c) => c.LoadsConstant(14f) ? nameof(SpeciesSettingsMinDatingAge) : c.LoadsConstant(16f) ? nameof(SpeciesSettingsMinLovinAge) : null;

  public static float GetAgeAndStorePawn(Pawn pawn)
  {
    cachedPawn = pawn;
    return pawn.ageTracker.AgeBiologicalYearsFloat;
  }

  public static float SpeciesSettingsMinDatingAge() => SpeciesSettingsMinRomanceAge(true);
  
  public static float SpeciesSettingsMinLovinAge() => SpeciesSettingsMinRomanceAge(false);

  //public static float SpeciesSettingsMinDatingAge(Pawn pawn) => SpeciesSettingsMinRomanceAge(pawn, true);

  //public static float SpeciesSettingsMinLovinAge(Pawn pawn) => SpeciesSettingsMinRomanceAge(pawn, false);

  //public static float SpeciesSettingsMinRomanceAge(Pawn pawn, bool isDating)
  //{
  //  if (!SpeciesHelper.RomanceEnabled(pawn, isDating))
  //  {
  //    return pawn?.ageTracker?.AgeBiologicalYearsFloat + 1f ?? 0f;
  //  }
  //  SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
  //  float romanceAge = isDating ? settings.minDatingAge : settings.minLovinAge;
  //  return romanceAge < 0f ? pawn.ageTracker.AgeBiologicalYearsFloat + 1f : romanceAge;
  //}

  public static float SpeciesSettingsMinRomanceAge(bool isDating)
  {
    if (!SpeciesHelper.RomanceEnabled(cachedPawn, isDating))
    {
      return cachedPawn.ageTracker.AgeBiologicalYearsFloat + 1f;
    }
    SpeciesSettings settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(cachedPawn.def);
    float romanceAge = isDating ? settings.minDatingAge : settings.minLovinAge;
    float result = romanceAge < 0f ? cachedPawn.ageTracker.AgeBiologicalYearsFloat + 1f : romanceAge;
    cachedPawn = null;
    return result;
  }

}

