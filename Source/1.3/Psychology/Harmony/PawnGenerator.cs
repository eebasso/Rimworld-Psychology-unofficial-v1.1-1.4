using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    public static class PawnGenerator_GenerateTraitsPatch
    {
        [LogPerformance]
        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static bool KinseyException(ref Pawn pawn, PawnGenerationRequest request)
        {
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                if (PsychologyBase.ActivateKinsey())
                {
                    //while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating > 2 && !request.AllowGay)
                    //{
                    //    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                    //}
                    if (!request.AllowGay)
                    {
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(1f, 1f, 0f, 0f, 0f, 0f, 0f);
                    }
                    if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
                    {
                        //while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating < 2)
                        //{
                        //    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                        //}
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(0f, 0f, 1f, 1f, 1f, 1f, 1f);
                    }
                    else if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                    {
                        //while (PsycheHelper.Comp(pawn).Sexuality.kinseyRating > 4)
                        //{
                        //    PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality();
                        //}
                        PsycheHelper.Comp(pawn).Sexuality.GenerateSexuality(1f, 1f, 1f, 1f, 1f, 0f, 0f);
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateTraits")]
    public static class PawnGenerator_GenerateTraitsSiblingsPatch
    {
        [HarmonyPostfix]
        public static void TaraiSiblings(ref Pawn pawn, ref PawnGenerationRequest request)
        {
            Pawn p = pawn;
            if(pawn.story != null && pawn.story.childhood == PsychologyBase.child)
            {
                IEnumerable<Pawn> other = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                              where x.def == p.def && x.story != null && x.story.childhood == p.story.childhood
                              select x);
                if(other.Count() > 0)
                {
                    Traverse.Create(typeof(PawnGenerator)).Field("relationsGeneratableBlood").GetValue<PawnRelationDef[]>().Where(r => r.defName == "Sibling").First().Worker.CreateRelation(pawn, other.First(), ref request);
                }
            }
        }
    }
}
