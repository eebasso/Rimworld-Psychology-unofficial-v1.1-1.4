using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;
using KTrie;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(RestUtility), nameof(RestUtility.CanFallAsleep))]
public static class RestUtility_CallFallAsleepPatch
{
  /// <summary>
  /// An untreated insomniac only has a chance to be able to sleep even if all other conditions are met, set by insomniacSleepChance field
  /// </summary>
  /// <param name="pawn"></param>
  /// <param name="__result"></param>
  [HarmonyPostfix]
  public static void CanFallAsleepPostfix(Pawn pawn, ref bool __result)
  {
    if (pawn.IsUntreatedInsomniac())
    {
      __result = PsycheHelper.InsomniacCanFallAsleep(pawn);
      //if (p.needs?.rest != null && p.needs.rest.TicksAtZero < 0)
      //{
      //  __result = false;
      //}
      //else if (__result)
      //{
      //  __result = Rand.ChanceSeeded(PsycheHelper.InsomniacRandRangeValueSeeded(1f, 0.6f));
      //}
    }
    else if (pawn?.story?.traits?.HasTrait(TraitDefOfPsychology.HeavySleeper) == true)
    {
      __result = true;
    }
  }
}

[HarmonyPatch(typeof(RestUtility), "WakeThreshold")]
public static class RestUtility_WakeThresholdPatch
{
  [HarmonyPostfix]
  public static void WakeThresholdPostfix(Pawn p, ref float __result)
  {
    __result = RestUtilityPatchHelper.FallAsleepMaxLevelTraitAdjustments(__result, p);
  }

}

[HarmonyPatch(typeof(RestUtility), "FallAsleepMaxLevel")]
public static class RestUtility_FallAsleepMaxLevelPatch
{
  //[HarmonyTranspiler]
  //public static IEnumerable<CodeInstruction> FallAsleepMaxLevelTranspiler(IEnumerable<CodeInstruction> codes)
  //{
  //  foreach (CodeInstruction c in codes)
  //  {
  //    yield return c;
  //    if (c.LoadsConstant(0.75f))
  //    {
  //      yield return new CodeInstruction(OpCodes.Ldarg_0);
  //      yield return CodeInstruction.Call(typeof(RestUtilityPatchHelper), nameof(RestUtilityPatchHelper.FallAsleepMaxLevelTraitAdjustments));
  //    }
  //  }
  //}

  [HarmonyPostfix]
  public static void FallAsleepMaxLevel(Pawn p, ref float __result)
  {
    __result = RestUtilityPatchHelper.FallAsleepMaxLevelTraitAdjustments(__result, p);
  }

}

[HarmonyPatch(typeof(RestUtility), nameof(RestUtility.DisturbancePreventsLyingDown))]
public static class RestUtility_DisturbancePreventsLyingDownPatch
{
  public static IEnumerable<CodeInstruction> DisturbancePreventsLyingDownTranspiler(IEnumerable<CodeInstruction> codes)
  {
    bool failure = true;
    FieldInfo fieldInfo = AccessTools.Field(typeof(Pawn_MindState), nameof(Pawn_MindState.lastDisturbanceTick));
    foreach (CodeInstruction c in codes)
    {
      yield return c;
      if (c.LoadsConstant())
      {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return CodeInstruction.Call(typeof(RestUtility_DisturbancePreventsLyingDownPatch), nameof(SleeperTraitsExceptions));
        failure = false;
      }
    }
    if (failure)
    {
      Log.Error("DisturbancePreventsLyingDownTranspiler: patch failed");
    }
  }

  public static int SleeperTraitsExceptions(int oldConstant, Pawn p)
  {
    if (p?.story?.traits?.HasTrait(TraitDefOfPsychology.HeavySleeper) == true)
    {
      return -1;
    }
    if (p.IsUntreatedInsomniac())
    {
      return Mathf.RoundToInt(oldConstant * PsycheHelper.InsomniacRandRangeValueSeeded(p, 1f, 3f));
    }
    return oldConstant;
  }


}

public static class RestUtilityPatchHelper
{
  public static float FallAsleepMaxLevelTraitAdjustments(float rate, Pawn p)
  {
    TraitSet traitSet;
    if ((traitSet = p?.story?.traits) != null)
    {
      if (traitSet.HasTrait(TraitDefOfPsychology.HeavySleeper))
      {
        rate = Mathf.Clamp(rate, 0.95f, 1f);
      }
      else if (p.IsUntreatedInsomniac())
      {
        rate *= PsycheHelper.InsomniacRandRangeValueSeeded(p, 1f, 0.5f);
      }
    }
    return rate;
  }
}