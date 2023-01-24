﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
  [HarmonyPatch(typeof(Pawn_StoryTracker))]
  [HarmonyPatch(nameof(Pawn_StoryTracker.TitleShort), MethodType.Getter)]
  public static class Pawn_StoryTracker_MayorLabel
  {

    [HarmonyPostfix]
    //public static void SetMayorLabel(Pawn_StoryTracker __instance, ref String __result)
    public static void SetMayorLabel(ref String __result, Pawn ___pawn)
    {
      //Pawn p = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
      if (___pawn != null && ___pawn.health.hediffSet.HasHediff(HediffDefOfPsychology.Mayor))
      {
        __result = "MayorTitle".Translate();
      }
    }
  }
}
