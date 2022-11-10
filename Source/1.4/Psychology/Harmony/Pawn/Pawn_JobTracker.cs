using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
public static class Pawn_JobTracker_EndCurrentJobPatch
{

  [HarmonyPrefix]
  public static bool HeavySleeperTrait(Pawn_JobTracker __instance, Pawn ___pawn)
  {
    if (!PsycheHelper.PsychologyEnabled(___pawn))
    {
      return true;
    }
    if (__instance?.curDriver?.asleep != true || ___pawn?.needs?.rest == null)
    {
      return true;
    }

    try
    {
      if (___pawn?.story?.traits?.HasTrait(TraitDefOfPsychology.HeavySleeper) != true)
      {
        return true;
      }
    }
    catch (Exception ex)
    {
      Log.Error("Psychology: Pawn_JobTracker_EndCurrentJobPatch HasTrait exception: " + ex);
    }
    
    try
    {
      if (Traverse.Create(___pawn.needs.rest).Field("lastRestTick").GetValue<int>() < Find.TickManager.TicksGame - 200)
      {
        return true;
      }
      return false;
    }
    catch (Exception ex)
    {
      Log.Error("Psychology: Pawn_JobTracker_EndCurrentJobPatch lastRestTick exception: " + ex);
      return true;
    }
  }

  //[HarmonyPrefix]
  //public static bool HeavySleeperTrait(Pawn_JobTracker __instance)
  //{
  //    Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
  //    return __instance.curDriver == null
  //        || !pawn.RaceProps.Humanlike
  //        || !__instance.curDriver.asleep
  //        || Traverse.Create(pawn.needs.rest).Field("lastRestTick").GetValue<int>() < Find.TickManager.TicksGame - 200
  //        || !pawn.story.traits.HasTrait(TraitDefOfPsychology.HeavySleeper);
  //}
}
