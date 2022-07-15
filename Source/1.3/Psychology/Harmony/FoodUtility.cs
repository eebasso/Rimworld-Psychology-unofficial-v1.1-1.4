using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harm
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting))]
    public static class FoodUtility_AddPickyThoughts_Patch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void AddPickyThoughtsPatch(ref List<FoodUtility.ThoughtFromIngesting> __result) // TEST in 1.3
        {
            List<FoodUtility.ThoughtFromIngesting> newIngestingThoughts = new List<FoodUtility.ThoughtFromIngesting>();

            foreach (var ingestingThought in __result)
            {
                if (DefDatabase<ThoughtDef>.GetNamedSilentFail(ingestingThought.thought.defName + "PickyEater") != null)
                {
                    FoodUtility.ThoughtFromIngesting newIngestingThought = new FoodUtility.ThoughtFromIngesting();
                    ThoughtDef thought = ThoughtDef.Named(ingestingThought.thought.defName + "PickyEater");
                    newIngestingThought.thought = thought;
                    // TODO : add percept thoughs??
                    newIngestingThoughts.Add(newIngestingThought);
                    //FoodUtility.ThoughtFromIngesting newIngestingThought = FoodUtility.ThoughtFromIngesting
                }

            }
            __result.AddRange(newIngestingThoughts);
        }
    }
}