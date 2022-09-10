/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Thing), "Ingested")]
    public static class Thing_IngestedThoughtsPatch
    {
        private static List<Thought_Memory> preIngestingMemories = null;

        [HarmonyPrefix]
        public static bool SavePreIngestingThoughts(this Thing __instance, Pawn ingester, float nutritionWanted)
        {
            preIngestingMemories = null;
            if (ingester.needs.mood != null)
            {
                preIngestingMemories = ingester.needs.mood.thoughts.memories.Memories;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void AddIngestingThoughtsToRecruitingMemories(this Thing __instance, Pawn ingester, float nutritionWanted)
        {
            if (preIngestingMemories != null && PsycheHelper.Comp(ingester) != null && (ingester as PsychologyPawn).recruiting != null)
            {
                IEnumerable<Thought_Memory> changedThoughts = (from Thought_Memory m in ingester.needs.mood.thoughts.memories.Memories
                                                               where !preIngestingMemories.Contains(m)
                                                               select m);
                PsychologyPawn realPawn = ingester as PsychologyPawn;
                foreach (Thought_Memory t in changedThoughts)
                {
                    realPawn.recruiting.avgFoodThoughts = new Pair<float, int>(realPawn.recruiting.avgFoodThoughts.First + t.CurStage.baseMoodEffect, realPawn.recruiting.avgFoodThoughts.Second + 1);
                }
            }
        }
    }
}
*/