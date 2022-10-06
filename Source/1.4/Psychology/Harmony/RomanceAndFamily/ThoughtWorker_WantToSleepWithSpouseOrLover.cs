//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using RimWorld.Planet;
//using Verse;
//using HarmonyLib;

//namespace Psychology.Harmony;

/* This patch appears no longer necessary because of how CurrentStateInternal is now written */

//[HarmonyPatch(typeof(ThoughtWorker_WantToSleepWithSpouseOrLover), "CurrentStateInternal")]
//public class ThoughtWorker_WantToSleepWithSpouseOrLoverPatch
//{

//    [HarmonyPostfix]
//    public static void CurrentStateInternal(ref ThoughtState __result, Pawn p)
//    {
//        if (__result.StageIndex == ThoughtState.Inactive.StageIndex)
//        {
//            return;
//        }
//        if (p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) != true)
//        {
//            return;
//        }
//        List<DirectPawnRelation> list = LovePartnerRelationUtility.ExistingLovePartners(p, false);
//        if (list.NullOrEmpty())
//        {
//            return;
//        }
//        if (list.Count() < 2)
//        {
//            return;
//        }
//        if (p.ownership?.OwnedBed == null)
//        {
//            return;
//        }
//        //DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, false);
//        bool partnerBedInRoom = (from t in p.ownership.OwnedBed.GetRoom().ContainedBeds
//                                 where t.OwnersForReading.Contains(directPawnRelation.otherPawn)
//                                 select t).Count() > 0;
//        if (directPawnRelation != null && p.ownership.OwnedBed != null && p.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && multiplePartners && partnerBedInRoom)
//        {
//            __result = false;
//        }
//    }

//}
