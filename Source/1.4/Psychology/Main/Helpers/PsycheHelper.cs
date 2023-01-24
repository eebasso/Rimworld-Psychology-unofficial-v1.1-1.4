using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Psychology;

public static class PsycheHelper
{
  public static PsychologyGameComponent GameComp => Current.Game.GetComponent<PsychologyGameComponent>();
  public static readonly Dictionary<KinseyMode, float[]> KinseyModeWeightDict = new Dictionary<KinseyMode, float[]>()
  {
    { KinseyMode.Realistic    , new float[] { 62.4949f, 11.3289f,  9.2658f,  6.8466f,  4.5220f,  2.7806f,  2.7612f } },
    { KinseyMode.Invisible    , new float[] {  7.0701f, 11.8092f, 19.5541f, 23.1332f, 19.5541f, 11.8092f,  7.0701f } },
    { KinseyMode.Uniform      , new float[] { 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f, 14.2857f } },
    { KinseyMode.Gaypocalypse , new float[] {  2.7612f,  2.7806f,  4.5220f,  6.8466f,  9.2658f, 11.3289f, 62.4949f } },
  };

  public static HashSet<string> TraitDefNamesThatAffectPsyche = new HashSet<string>();

  public static Dictionary<Gender, Dictionary<int, float>> GenderModifierNodeDefDict = new Dictionary<Gender, Dictionary<int, float>>();
  public static Dictionary<SkillDef, HashSet<int>> SkillModifierNodeDefDict = new Dictionary<SkillDef, HashSet<int>>();
  public static Dictionary<Pair<TraitDef, int>, Dictionary<int, float>> TraitModifierNodeDefDict = new Dictionary<Pair<TraitDef, int>, Dictionary<int, float>>();
  public static Dictionary<WorkTypeDef, Dictionary<int, float>> IncapableModifierNodeDefDict = new Dictionary<WorkTypeDef, Dictionary<int, float>>();

  //public static HashSet<string> SkillDefNamesThatAffectPsyche = new HashSet<string>();
  public static int seed;
  public static float DailyCertaintyChangeScale => 0.0015f * PsychologySettings.ideoPsycheMultiplier;
  public static CompPsychology comp;
  public static SpeciesSettings settings;
  public static bool flag;
  public static float[] CircumstanceTimings = new float[25];
  public static int CircumstanceCount = 0;

  //public static int countPsychologyEnabled;
  //public static float msPsychologyEnabled;

  public static bool PsychologyEnabled(Pawn pawn)
  {
    //Stopwatch stopwatch = new Stopwatch();
    //stopwatch.Start();
    if (!HasLatentPsyche(pawn))
    {
      Log.Message("PsychologyEnabled, HasLatentPsyche != true, pawn = " + pawn.Label + ", species label = " + pawn.def.label);
      return false;
    }
    if (!SpeciesSettingsEnablePsyche(pawn))
    {
      Log.Message("PsychologyEnabled, SpeciesSettingsEnablePsyche != true");
      return false;
    }
    if (!IsSapient(pawn))
    {
      Log.Message("PsychologyEnabled, IsSapient != true, pawn = " + pawn.Label + ", species label = " + pawn.def.label);
      return false;
    }
    //stopwatch.Stop();
    //TimeSpan ts = stopwatch.Elapsed;
    //countPsychologyEnabled++;
    //msPsychologyEnabled += (float)ts.TotalMilliseconds;
    ////Log.Message("PsychologyEnabled, total time: " + msPsychologyEnabled + " ms, average time: " + msPsychologyEnabled / countPsychologyEnabled + " ms");
    Log.Message("PsychologyEnabled, return true");
    return true;
  }

  public static bool HasLatentPsyche(Pawn pawn)
  {
    if (pawn == null)
    {
      Log.Warning("HasLatentPsyche, pawn == null");
      return false;
    }
    Log.Message("HasLatentPsyche, pawn != null");
    comp = Comp(pawn);
    if (comp == null)
    {
      if (!SpeciesHelper.CheckIntelligenceAndAddEverythingToSpeciesDef(pawn.def))
      {
        Log.Message("HasLatentPsyche, Comp(pawn) == null, pawn = " + pawn + ", species = " + pawn.def);
        return false;
      }
      comp = Comp(pawn);
      if (comp == null)
      {
        Log.Error("HasLatentPsyche, comp == null after CheckIntelligenceAndAddEverythingToHumanlikeDef == true");
        return false;
      }
    }
    Log.Message("HasLatentPsyche, Comp(pawn) != null");
    if (!comp.IsPsychologyPawn)
    {
      Log.Message("HasLatentPsyche, IsPsychologyPawn != true, pawn = " + pawn + ", species = " + pawn.def);
      return false;
    }
    return true;
  }

  public static bool SpeciesSettingsEnablePsyche(Pawn pawn)
  {
    settings = SpeciesHelper.GetOrMakeSpeciesSettingsFromThingDef(pawn.def);
    return settings.enablePsyche;
    //if (settings.enablePsyche != true)
    //{
    //  //L//Log.Message"PsychologyEnabled, settings.enablePsyche != true, " + pawn + ", species = " + pawn.def);
    //  return false;
    //}
    ////L//Log.Message"PsychologyEnabled, settings.enablePsyche == true");
    //return true;
  }

  public static bool IsSapient(Pawn pawn)
  {
    return SpeciesHelper.IsHumanlikeIntelligence(pawn.def);
  }

  public static CompPsychology Comp(Pawn pawn)
  {
    return pawn.GetComp<CompPsychology>();
  }

  public static void Look<T>(ref HashSet<T> valueHashSet, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
  {
    List<T> list = null;
    if (Scribe.mode == LoadSaveMode.Saving && valueHashSet != null)
    {
      list = new List<T>();
      foreach (T current in valueHashSet)
      {
        list.Add(current);
      }
    }
    Scribe_Collections.Look<T>(ref list, false, label, lookMode, ctorArgs);
    if ((lookMode == LookMode.Reference && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (lookMode != LookMode.Reference && Scribe.mode == LoadSaveMode.LoadingVars))
    {
      if (list == null)
      {
        valueHashSet = null;
      }
      else
      {
        valueHashSet = new HashSet<T>();
        for (int i = 0; i < list.Count; i++)
        {
          valueHashSet.Add(list[i]);
        }
      }
    }
  }

  public static void InitializeDictionariesForPersonalityNodeDefs()
  {
    Dictionary<PersonalityNodeDef, int> indexDict = PersonalityNodeMatrix.indexDict;
    int index;

    foreach (Gender gender in Gender.GetValues(typeof(Gender)))
    {
      GenderModifierNodeDefDict[gender] = new Dictionary<int, float>();
    }

    foreach (PersonalityNodeDef pDef in PersonalityNodeMatrix.DefList)
    {
      if (pDef.femaleModifier != default && pDef.femaleModifier != 0f)
      {
        index = indexDict[pDef];
        GenderModifierNodeDefDict[Gender.Male][index] = -pDef.femaleModifier;
        GenderModifierNodeDefDict[Gender.Female][index] = pDef.femaleModifier;
      }

      if (pDef.skillModifiers.NullOrEmpty() != true)
      {
        foreach (PersonalityNodeSkillModifier skillMod in pDef.skillModifiers)
        {
          if (SkillModifierNodeDefDict.ContainsKey(skillMod.skill) != true)
          {
            SkillModifierNodeDefDict[skillMod.skill] = new HashSet<int>();
          }
          SkillModifierNodeDefDict[skillMod.skill].Add(indexDict[pDef]);
        }
      }

      if (pDef.traitModifiers.NullOrEmpty() != true)
      {
        foreach (PersonalityNodeTraitModifier traitMod in pDef.traitModifiers)
        {
          TraitDefNamesThatAffectPsyche.Add(traitMod.trait.defName);
          Pair<TraitDef, int> pair = new Pair<TraitDef, int>(traitMod.trait, traitMod.degree);
          if (TraitModifierNodeDefDict.ContainsKey(pair) != true)
          {
            TraitModifierNodeDefDict[pair] = new Dictionary<int, float>();
          }
          TraitModifierNodeDefDict[pair][indexDict[pDef]] = traitMod.modifier;
        }
      }

      if (pDef.incapableModifiers.NullOrEmpty() != true)
      {
        foreach (PersonalityNodeIncapableModifier incapableMod in pDef.incapableModifiers)
        {
          if (IncapableModifierNodeDefDict.ContainsKey(incapableMod.type) != true)
          {
            IncapableModifierNodeDefDict[incapableMod.type] = new Dictionary<int, float>();
          }
          IncapableModifierNodeDefDict[incapableMod.type][indexDict[pDef]] = incapableMod.modifier;
        }
      }
    }
  }

  public static float DatingBioAgeToVanilla(float bioAge, float minDatingAge) => bioAge * 14f / minDatingAge;

  public static float LovinBioAgeToVanilla(float bioAge, float minLovinAge) => bioAge * 16f / minLovinAge;

  public static float DatingBioAgeFromVanilla(float vanillaAge, float minDatingAge) => vanillaAge * minDatingAge / 14f;

  public static float LovinBioAgeFromVanilla(float vanillaAge, float minLovinAge) => vanillaAge * minLovinAge / 16f;

  public static float RandGaussianSeeded(int specialSeed1, int specialSeed2, float centerX = 0f, float widthFactor = 1f)
  {
    float value = Rand.ValueSeeded(specialSeed1);
    float value2 = Rand.ValueSeeded(specialSeed2);
    return Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin((float)Math.PI * 2f * value2) * widthFactor + centerX;
  }

  public static float RandGaussianSeeded(int specialSeed, float centerX = 0f, float widthFactor = 1f)
  {
    float value = Rand.ValueSeeded(specialSeed);
    return NormalCDFInv(value) * widthFactor + centerX;
  }

  /// <summary>
  /// This saddle shaped function is used to calculate the opinion modifier due to differences in personality rating on a given conversation topic.
  /// It reaches its maximum of +1 when <paramref name="x"/> = <paramref name="y"/> = 0 or 1 (complete agreement on a topic), and its minimum of -1 when <paramref name="x"/> = 0 and <paramref name="y"/> = 1 or vice versa (complete disagreement on a topic).
  /// This function is invariant under both {<paramref name="x"/>, <paramref name="y"/>} --> {<paramref name="y"/>, <paramref name="x"/>} and {<paramref name="x"/>, <paramref name="y"/>} --> {1 - <paramref name="x"/>, 1 - <paramref name="y"/>} transformations.
  /// The inputs <paramref name="x"/> and <paramref name="y"/> are the ratings ranging between 0 and 1, while <paramref name="f0"/> sets the value of the function when <paramref name="x"/> = <paramref name="y"/> = 1/2 and <paramref name="gamma"/> controls how fast agreement drops off as a function of the difference between <paramref name="x"/> and <paramref name="y"/>.
  /// </summary>
  /// <param name="x">A rating between 0 and 1.</param>
  /// <param name="y">A rating between 0 and 1.</param>
  /// <param name="f0">Sets the value of the function when <paramref name="x"/> = <paramref name="y"/> = 1/2. Should range between 0 and 1.</param>
  /// <param name="gamma">Controls how fast agreement drops off as a function of the difference between <paramref name="x"/> and <paramref name="y"/>. Should be non-negative.</param>
  /// <returns>Number between -1 and +1 representing the degree of agreement (or disagreement if negative) between <paramref name="x"/> and <paramref name="y"/>.</returns>
  public static float SaddleShapeFunction(float x, float y, float f0 = 0.5f, float gamma = 4f)
  {
    float a = 1f + f0 + gamma;
    float b = 1f - f0;
    float diff = x - y;
    float diff2 = diff * diff;
    float sum = x + y - 1f;
    float sum2 = sum * sum;
    return (f0 - a * diff2 + b * sum2) / (1f + gamma * diff2);
    //return (f0 - (1f + f0 + gamma) * Mathf.Pow(x - y, 2) + (1f - f0) * Mathf.Pow(x + y - 1f, 2)) / (1f + gamma * Mathf.Pow(x - y, 2));
  }

  /// <summary>
  /// Converts <paramref name="normalVariable"/>, a normally distributed variable with a mean of 0 and a variance of 1, to a uniformly distributed rating between 0 and 1.
  /// </summary>
  /// <param name="normalVariable">A normally distributed variable with mean of 0 and a variance of 1. Ranges from negative to positive infinity.</param>
  /// <returns>A uniformly distributed rating ranging between 0 and 1.</returns>
  public static float NormalCDF(float normalVariable)
  {
    // constants
    double a1 = 0.127414796;
    double a2 = -0.142248368;
    double a3 = 0.710706871;
    double a4 = -0.726576014;
    double a5 = 0.530702716;
    double p = 0.231641888;
    // Save the sign of x
    bool sign = normalVariable > 0;
    double x = Math.Abs((double)normalVariable);
    // A&S formula 7.1.26
    double t = 1f / (1f + p * x);
    double z = (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-0.5f * x * x);
    return (float)(sign ? 1 - z : z);
  }






  /// <summary>
  /// Converts <paramref name="ratingBetween0and1"/>, a uniformly distributed rating between 0 and 1, to a normally distributed variable with a mean of 0 and variance of 1.
  /// </summary>
  /// <param name="ratingBetween0and1">A uniformly distributed rating between 0 and 1.</param>
  /// <param name="minRating">Minimum clamped value of <paramref name="ratingBetween0and1"/>, used to avoid divergenece.</param>
  /// <param name="maxRating">Maximum clamped value of <paramref name="ratingBetween0and1"/>, used to avoid divergencce.</param>
  /// <returns>A normally distributed variable with mean of 0 and a variance of 1. The minimum and maximum possible values are affected by <paramref name="minRating"/> and <paramref name="maxRating"/>, respecitvely.</returns>
  public static float NormalCDFInv(float ratingBetween0and1, float minRating = 0.0001f, float maxRating = 0.9999f)
  {
    ratingBetween0and1 = Mathf.Clamp(ratingBetween0and1, minRating, maxRating);
    if (ratingBetween0and1 < 0.5f)
    {
      return -RationalApproximation(Math.Sqrt(-2 * Math.Log(ratingBetween0and1)));
    }
    else
    {
      return RationalApproximation(Math.Sqrt(-2 * Math.Log(1 - ratingBetween0and1)));
    }
  }

  /// <summary>
  /// Abramowitz and Stegun formula 26.2.23. The absolute value of the error should be less than 4.5 e-4.
  /// </summary>
  /// <param name="t"></param>
  /// <returns></returns>
  public static float RationalApproximation(double t)
  {
    //double t = (double)t0;
    double[] c = { 2.515517, 0.802853, 0.010328 };
    double[] d = { 1.432788, 0.189269, 0.001308 };
    double result = t - ((c[2] * t + c[1]) * t + c[0]) / (((d[2] * t + d[1]) * t + d[0]) * t + 1f);
    return (float)result;
  }

  public static void CorrectTraitsForPawnKinseyEnabled(Pawn pawn)
  {
    if (pawn?.story?.traits == null)
    {
      return;
    }
    if (TryRemoveTraitDef(pawn, TraitDefOf.Asexual))
    {
      Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Asexual trait from pawn = " + pawn.Label);
      if (!PsychologyEnabled(pawn))
      {
        return;
      }
      Comp(pawn).Sexuality.AsexualTraitReroll();
    }
    if (TryRemoveTraitDef(pawn, TraitDefOf.Bisexual))
    {
      Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Bisexual trait from pawn = " + pawn.Label);
      if (!PsychologyEnabled(pawn))
      {
        return;
      }
      Comp(pawn).Sexuality.BisexualTraitReroll();
    }
    if (TryRemoveTraitDef(pawn, TraitDefOf.Gay))
    {
      Log.Warning("CorrectTraitsForPawnKinseyEnabled, Removed Gay trait from pawn = " + pawn.Label);
      if (!PsychologyEnabled(pawn))
      {
        return;
      }
      Comp(pawn).Sexuality.GayTraitReroll();
    }
  }

  public static void CorrectTraitsForPawnKinseyDisabled(Pawn pawn)
  {
    if (!PsycheHelper.PsychologyEnabled(pawn) || pawn?.story?.traits == null)
    {
      return;
    }
    int kinseyRating = PsycheHelper.Comp(pawn).Sexuality.kinseyRating;
    if (PsycheHelper.Comp(pawn).Sexuality.sexDrive < 0.1f)
    {
      TryGainTraitDef(pawn, TraitDefOf.Asexual);
    }
    if (kinseyRating < 2)
    {
      // If pawn is mostly heterosexual
      TryRemoveTraitDef(pawn, TraitDefOf.Bisexual);
      TryRemoveTraitDef(pawn, TraitDefOf.Gay);
    }
    else if (kinseyRating < 5)
    {
      // If pawn is mostly bisexual
      TryGainTraitDef(pawn, TraitDefOf.Bisexual);
      TryRemoveTraitDef(pawn, TraitDefOf.Gay);
    }
    else
    {
      // If pawn is mostly homosexual
      TryRemoveTraitDef(pawn, TraitDefOf.Bisexual);
      TryGainTraitDef(pawn, TraitDefOf.Gay);
    }
  }

  public static bool HasTraitDef(Pawn pawn, TraitDef traitDef)
  {
    foreach (Trait trait in pawn.story.traits.allTraits)
    {
      if (trait.def == traitDef)
      {
        return true;
      }
    }
    return false;
  }

  public static bool TryGainTraitDef(Pawn pawn, TraitDef traitDef)
  {
    if (HasTraitDef(pawn, traitDef) != true)
    {
      return false;
    }
    pawn.story.traits.GainTrait(new Trait(traitDef));
    return true;
  }

  public static bool TryRemoveTraitDef(Pawn pawn, TraitDef traitDef)
  {
    foreach (Trait trait in pawn.story.traits.allTraits)
    {
      if (trait.def == traitDef)
      {
        pawn.story.traits.RemoveTrait(trait);
        return true;
      }
    }
    return false;
  }

  public static int PawnSeed(Pawn pawn)
  {
    int thingIDSeed = pawn.thingIDNumber;
    int worldIDSeed = Find.World.info.Seed;
    return Gen.HashCombineInt(thingIDSeed, worldIDSeed);
    //if (TryGetPawnSeed(pawn) != true)
    //{
    //    string thingID = pawn?.ThingID != null ? pawn?.ThingID : "null";
    //    string label = pawn?.Label != null ? pawn?.Label : "null";
    //    string defName = pawn?.def?.defName != null ? pawn?.def?.defName : "null";
    //    Log.Error("Used random pawn seed, would prefer this is not happen, thingID = " + thingID + ", + pawn.Label = " + label + ", pawn.def.defName = " + defName);
    //}
    //return seed;
  }

  public static bool TryGetPawnSeed(Pawn pawn)
  {
    return true;
  }

  public static void TryGainMemoryReplacedPartBleedingHeart(Pawn pawn, Pawn billDoer)
  {
    if (billDoer?.needs?.mood != null)
    {
      billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
    }
  }

  //public static float[] KinseyProbabilities()
  //{
  //    float[] pList = new float[7];
  //    KinseyMode kinseyMode = PsychologySettings.kinseyFormula;
  //    if (kinseyMode != KinseyMode.Custom)
  //    {
  //        KinseyModeWeightDict[kinseyMode].CopyTo(pList, 0);
  //    }
  //    else
  //    {
  //        PsychologySettings.kinseyWeightCustom.ToArray().CopyTo(pList, 0);
  //    }
  //    float sum = pList.Sum();
  //    if (sum == 0f)
  //    {
  //        pList = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f };
  //        sum = 7f;
  //    }
  //    for (int i = 0; i < 7; i++)
  //    {
  //        pList[i] = pList[i] / sum;
  //    }
  //    return pList;
  //}

  public static float RelativisticAddition(float u, float v)
  {
    return (u + v) / (1f + u * v);
  }

  public static float DaysShifted(Pawn pawn)
  {
    int daysPassed = GenLocalDate.DayOfYear(pawn) + GenDate.DaysPerYear * GenLocalDate.Year(pawn);
    // Shift local hour to reset at noon
    float hoursPastNoon = GenLocalDate.HourFloat(pawn) - 12f;
    return (float)daysPassed + hoursPastNoon / GenDate.HoursPerDay;
  }

  public static int InsomniacSeed(Pawn pawn, float days) => 5 * PawnSeed(pawn) + Mathf.FloorToInt(DaysShifted(pawn) / days);

  public static float InsomniacRandRangeValueSeeded(Pawn pawn, float valueMildSymptoms = 0, float valueSevereSymptoms = 1, float days = 3f)
  {
    return valueMildSymptoms + (valueSevereSymptoms - valueMildSymptoms) * Rand.ValueSeeded(InsomniacSeed(pawn, days));
  }

  public static bool InsomniacCanFallAsleep(Pawn pawn)
  {
    float chanceToSleepPerInterval = InsomniacRandRangeValueSeeded(pawn, 1f, 0f);
    float checkIntervalInHours = InsomniacRandRangeValueSeeded(pawn, 2f, 4f, 0.25f);
    int checkIntervalSeed = InsomniacSeed(pawn, checkIntervalInHours / GenDate.HoursPerDay);
    return Rand.ChanceSeeded(chanceToSleepPerInterval, checkIntervalSeed);
  }




}

