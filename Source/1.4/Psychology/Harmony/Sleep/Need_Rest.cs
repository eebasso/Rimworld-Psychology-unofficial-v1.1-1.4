using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
public static class Need_Rest_NeedIntervalPatch
{
  //[HarmonyTranspiler]
  //public static IEnumerable<CodeInstruction> NeedIntervalTranspiler(IEnumerable<CodeInstruction> codes)
  //{
  //  List<CodeInstruction> c = codes.ToList();
  //  FieldInfo lastRestFieldInfo = AccessTools.Field(typeof(Need_Rest), "lastRestEffectiveness");
  //  FieldInfo ticksAtZeroFieldInfo = AccessTools.Field(typeof(Need_Rest), "ticksAtZero");
  //  int n = c.Count();
  //  for (int i = 0; i < n; i++)
  //  {
  //    yield return c[i];
  //    if (c[i].LoadsField(lastRestFieldInfo))
  //    {
  //      yield return new CodeInstruction(OpCodes.Ldarg_0);
  //      yield return CodeInstruction.LoadField(typeof(Need), "pawn");
  //      yield return CodeInstruction.Call(typeof(Need_Rest_NeedIntervalPatch), nameof(RestEffectivenessInsomniacAdjustment));
  //    }
  //    //if (c[i].LoadsConstant(0) && i < n - 1 && c[i + 1].StoresField(ticksAtZeroFieldInfo))
  //    //{
  //    //  yield return new CodeInstruction(OpCodes.Pop);
  //    //  yield return CodeInstruction.LoadField(typeof(Need_Rest), "ticksAtZero");
  //    //  yield return CodeInstruction.Call(typeof(Need_Rest_NeedIntervalPatch), nameof(KeepNegativeTicks));
  //    //}
  //  }
  //}

  ////public static int KeepNegativeTicks(int ticksAtZero) => Math.Min(0, ticksAtZero + NeedTunings.NeedUpdateInterval);

  //public static float RestEffectivenessInsomniacAdjustment(float rate, Pawn p)
  //{
  //  if (p.IsUntreatedInsomniac())
  //  {
  //    float multiplier = PsycheHelper.InsomniacRandRangeValueSeeded(p, 1f, 0.5f);
  //    Log.Message("Multiplied lastRestEffectiveness by " + multiplier + " for pawn " + p);
  //    rate *= multiplier;
  //  }
  //  return rate;
  //}

  [HarmonyPostfix]
  public static void NeedIntervalPostfix(Need_Rest __instance, Pawn ___pawn)
  {
    if (___pawn?.RaceProps?.Humanlike != true || ___pawn.Awake() || !__instance.Resting)
    {
      return;
    }
    CauseDreams(___pawn);
    InhibitSleepForInsomniacs(__instance, ___pawn);
  }

  public static void CauseDreams(Pawn pawn)
  {
    // ToDo: Perhaps make settings for rate of dreams and fraction of good dreams. This would replace 60f and 0.5f.
    // By default, the mean time between dreams is 60 hours, and the chance of a bad dream is 50%.
    if (Rand.MTBEventOccurs(60f, GenDate.TicksPerHour, NeedTunings.NeedUpdateInterval))
    {
      ThoughtDef dream = Rand.Value < 0.5f ? (Rand.Value < 0.125f ? ThoughtDefOfPsychology.DreamNightmare : ThoughtDefOfPsychology.DreamBad) : ThoughtDefOfPsychology.DreamGood;
      pawn.needs.mood.thoughts.memories.TryGainMemory(dream, pawn);
    }
  }

  public static void InhibitSleepForInsomniacs(Need_Rest instance, Pawn pawn)
  {
    if (!pawn.IsUntreatedInsomniac() || instance.CurLevel < Need_Rest.DefaultNaturalWakeThreshold / 4f)
    {
      return;
    }
    if (!PsycheHelper.InsomniacCanFallAsleep(pawn))
    {
      pawn.jobs.curDriver.asleep = false;
      pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
      //AccessTools.Field(typeof(Need_Rest), "ticksAtZero").SetValue(pawn.needs, (int)(GenDate.TicksPerHour * PsycheHelper.InsomniacRandRangeValueSeeded(pawn, 0f, -24f)));
    }
  }


}

//[HarmonyPatch(typeof(Need_Rest), "RestFallFactor", MethodType.Getter)]
//public static class Need_Rest_RestFallFactorPatch
//{
//  /// <summary>
//  /// Untreated insomniacs don't get tired as easily???
//  /// </summary>
//  /// <param name="___pawn"></param>
//  /// <param name="__result"></param>
//  [HarmonyPostfix]
//  public static void RestFallFactorPostfix(Pawn ___pawn, float __result)
//  {
//    if (___pawn.IsUntreatedInsomniac())
//    {
//      __result *= 0.667f; // ToDo: setting?
//      return;
//    }
//  }
//}

//[HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
//public static class Need_Rest_IntervalInsomniacPatch
//{

//  [HarmonyPostfix]
//  public static void MakeInsomniacLessRestful(Need_Rest __instance, Pawn ___pawn)
//  {
//    Pawn pawn = ___pawn;
//    //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
//    if (pawn?.RaceProps?.Humanlike != true)
//    {
//      return;
//    }
//    if (!(pawn.story?.traits?.HasTrait(TraitDefOfPsychology.Insomniac) == true) && !(pawn.health?.hediffSet?.HasHediff(HediffDefOfPsychology.SleepingPills) == true))
//    {
//      return;
//    }
//    if (!__instance.Resting || Traverse.Create(__instance).Property("IsFrozen").GetValue<bool>())
//    {
//      return;
//    }
//    __instance.CurLevel -= (2f * 150f * Need_Rest.BaseRestGainPerTick) / 3f;
//    if (__instance.CurLevel > (Need_Rest.DefaultNaturalWakeThreshold / 4f))
//    {
//      if (Rand.MTBEventOccurs((Need_Rest.DefaultNaturalWakeThreshold - __instance.CurLevel) / 4f, GenDate.TicksPerDay, 150f) && !pawn.Awake())
//      {
//        pawn.jobs.curDriver.asleep = false;
//        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
//      }
//    }
//  }
//}
