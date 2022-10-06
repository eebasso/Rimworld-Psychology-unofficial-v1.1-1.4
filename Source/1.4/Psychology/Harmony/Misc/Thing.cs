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

//using HarmonyLib;
//using RimWorld;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Reflection.Emit;
//using Verse;

//[HarmonyPatch(typeof(ITab_Pawn_Log), "FillTab")]
//public class ITab_Pawn_Log_Patches
//{
//    [HarmonyTranspiler]
//    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
//    {
//        Log.Message("step 0");
//        MethodInfo methodSelPawn = AccessTools.PropertyGetter(typeof(ITab), "SelPawn");
//        Log.Message("step 1");
//        yield return new CodeInstruction(OpCodes.Ldarg_0);
//        Log.Message("step 2");
//        yield return new CodeInstruction(OpCodes.Call, methodSelPawn);
//        Log.Message("step 3");
//        yield return CodeInstruction.Call(typeof(ITab_Pawn_Log_Patches), nameof(SomeMethod));
//        Log.Message("step 4");
//        foreach (CodeInstruction c in codes)
//        {
//            yield return c;
//        }
//        Log.Message("step 5");
//    }
//    public static void SomeMethod(Pawn pawn)
//    {
//        Log.Message("LOL this worked for pawn = " + pawn);
//    }
//}