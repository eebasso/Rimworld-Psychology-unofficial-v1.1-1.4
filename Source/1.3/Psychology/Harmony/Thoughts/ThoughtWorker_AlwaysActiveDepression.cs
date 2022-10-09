//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using HarmonyLib;

//namespace Psychology.Harmony;

//[HarmonyPatch(typeof(ThoughtWorker_AlwaysActive), nameof(ThoughtWorker_AlwaysActive.MoodMultiplier))]
//public static class ThoughtWorker_AlwaysActivePatch
//{
//    [HarmonyPostfix]
//    public static void MoodMultiplier(ref float __result, ThoughtWorker_AlwaysActive __instance, Pawn p)
//    {
//        if (p?.health?.hediffSet?.HasHediff(HediffDefOfPsychology.Antidepressants) != true)
//        {
//            return;
//        }
//        if (__instance.def.stages[0].baseOpinionOffset < 0f)
//        {
//            __result *= 0.5f;
//        }
//    }
//}

//[HarmonyPatch(typeof(ThoughtWorker_AlwaysActive), "CurrentStateInternal")]
//public static class ThoughtWorker_AlwaysActivePatch
//{
//    [HarmonyPostfix]
//    public static void AlwaysActiveDepression(ref ThoughtState __result, Pawn p)
//    {
//        if (__result.StageIndex > 1)
//        {
//            if (p.health.hediffSet.HasHediff(HediffDefOfPsychology.Antidepressants))
//            {
//                __result = ThoughtState.ActiveAtStage(1);
//            }
//        }
//    }
//}

