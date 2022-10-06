//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using HarmonyLib;

//namespace Psychology.Harmony;

//[HarmonyPatch(typeof(ThoughtWorker_ChemicalInterestVsTeetotaler), "CurrentSocialStateInternal")]
//public static class ThoughtWorker_ChemicalInterestVsTeetotalerPatch
//{
    
//    [HarmonyPostfix]
//    public static void Disable(ref ThoughtState __result, Pawn p, Pawn other)
//    {
//        if (__result.StageIndex != ThoughtState.Inactive.StageIndex)
//        {
//            if (PsychologySettings.traitOpinionMultiplier == 0f)
//            {
//                __result = false;
//            }
//        }
//    }
//}
