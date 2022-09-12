using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
public static class PawnGenerator_GenerateTraits_Patch
{
    [HarmonyPostfix]
    public static void TaraiSiblings_Postfix(Pawn pawn, PawnGenerationRequest request)
    {
        if (pawn.story == null || pawn.story.childhood != PsychologyDefInjector.child || PsycheHelper.GameComp.taraiSiblingsGenerated)
        {
            return;
        }
        foreach (Pawn otherPawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
        {
            bool arePawnsCompatible = pawn.def == otherPawn.def
                && pawn.story != null && otherPawn.story != null
                && pawn.story.childhood == otherPawn.story.childhood
                && pawn.gender != otherPawn.gender;
            if (!arePawnsCompatible)
            {
                continue;
            }
            PawnRelationDefOf.Sibling.Worker.CreateRelation(pawn, otherPawn, ref request);
            PsycheHelper.GameComp.taraiSiblingsGenerated = true;
            // we really only want to affect the first found pawn, so just return after we added a relation
            return;
        }
    }
}

public static class PawnGenerator_ManualPatches
{
    // Postfix
    //public static void GenerateTraits_KinseyEnabled(Pawn pawn, PawnGenerationRequest request)
    //{
    //    if (pawn.story == null || !PsycheHelper.PsychologyEnabledFast(pawn))
    //    {
    //        return;
    //    }
    //    PsycheHelper.CorrectTraitsForPawnKinseyEnabled(pawn);
    //}

    // Postfix


    public static void GeneratePawn_IdeoCache_Postfix(ref Pawn __result, PawnGenerationRequest request)
    {
        if (!PsycheHelper.PsychologyEnabled(__result))
        {
            return;
        }
        int idNumber = __result.thingIDNumber;
        if (!PsycheHelper.GameComp.CachedCertaintyChangePerDayDict.ContainsKey(idNumber))
        {
            float ideoCertaintyChange = PsycheHelper.Comp(__result).Psyche.CalculateCertaintyChangePerDay();
            PsycheHelper.GameComp.CachedCertaintyChangePerDayDict.Add(idNumber, ideoCertaintyChange);
        }
    }
}

//[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn))]
//public static class PawnGenerator_GeneratePawn_Patch
//{
//    [HarmonyPostfix]

//}

//[HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
//public static class PawnGenerator_GenerateTraitsSiblingsPatch
//{
//    [HarmonyPostfix]
//    public static void TaraiSiblings(ref Pawn pawn, ref PawnGenerationRequest request)
//    {
//        Pawn p = pawn;
//        if (pawn.story != null && pawn.story.childhood == PsychologyDefInjector.child)
//        {
//            IEnumerable<Pawn> other = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
//                                       where x.def == p.def && x.story != null && x.story.childhood == p.story.childhood
//                                       select x);
//            if (other.Count() > 0)
//            {
//                Traverse.Create(typeof(PawnGenerator)).Field("relationsGeneratableBlood").GetValue<PawnRelationDef[]>().Where(r => r.defName == "Sibling").First().Worker.CreateRelation(pawn, other.First(), ref request);
//            }
//        }
//    }
//}





