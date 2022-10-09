using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Psychology
{
    public static class RecruitingUtility
    {
        /*public static float Amenability(PsychologyPawn pawn, PsychologyPawn recruiter)
        {
            if(pawn.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.MyOrganHarvested) > 0
                || pawn.needs.mood.thoughts.memories.NumMemoriesOfDef(ThoughtDefOf.AteHumanlikeMeatAsIngredient) > 0)
            {
                recruiter.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Insulted, recruiter);
                return 0f;
            }
            float cellFactor = 0f;
            if(pawn.GetRoom().isPrisonCell)
            {
                cellFactor = 0.1f * Mathf.InverseLerp(0f, 75f, pawn.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
            }
            float friendFactor = 0f;
            foreach(Pawn c in pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if(pawn.relations.OpinionOf(c) > 20)
                {
                    friendFactor += (0.25f / pawn.Map.mapPawns.FreeColonistsSpawnedCount);
                }
            }
            float wardenFactor = 0.4f * recruiter.GetStatValue(StatDefOf.RecruitPrisonerChance, true);
            wardenFactor *= Mathf.Lerp(0.5f, 1.25f, (recruiter.psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly) + recruiter.psyche.GetPersonalityRating(PersonalityNodeDefOf.Empathetic))/2f);
            float tourFactor = 0.25f * pawn.recruiting.ColonyImpressiveness;
            float relationBonus = 0f;
            if(LovePartnerRelationUtility.LovePartnerRelationExists(pawn, recruiter) || pawn.relations.FamilyByBlood.Contains(recruiter))
            {
                relationBonus = 0.1f;
            }
            return Mathf.Lerp((cellFactor + friendFactor + wardenFactor + tourFactor + relationBonus), 0f, pawn.RecruitDifficulty(recruiter.Faction, true));
        }

        public static void PrepareForRecruiting(PsychologyPawn pawn)
        {
            if(pawn.recruiting == null)
                pawn.recruiting = new Pawn_TourMemories(pawn);
        }*/
    }
}
