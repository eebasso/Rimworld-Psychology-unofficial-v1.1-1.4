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

public static class PawnGenerator_ConditionalPatches
{
    public static void GenerateTraitsKinseyEnabled(ref Pawn pawn, PawnGenerationRequest request)
    {
        PsychologyGameComponent.CorrectTraitsForPawnKinseyEnabled(pawn);
    }

    //public static void GenerateTraitsKinseyDisabled(ref Pawn pawn, PawnGenerationRequest request)
    //{
    //    PsychologyGameComponent.CorrectTraitsForPawnKinseyDisabled(pawn);
    //}

    public static void GenerateTraitsTaraiSiblings(Pawn pawn, PawnGenerationRequest request)
    {
        if (pawn.story == null || pawn.story.childhood != PsychologyDefInjector.child)
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
            PsychologySettings.taraiSiblingsGenerated = true;
            // we really only want to affect the first found pawn, so just return after we added a relation
            return;
        }
    }




}

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





