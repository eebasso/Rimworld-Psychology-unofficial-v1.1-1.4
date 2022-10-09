using System;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Recipe_InstallImplant), nameof(Recipe_InstallImplant.ApplyOnPawn))]
    public static class Recipe_InstallImplant_ApplyPatch
    {
        // This transpiler hopefully accounts for surgery success / failure
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            bool flag = true;
            foreach (CodeInstruction c in codes)
            {
                yield return c;
                if (flag && c.opcode == OpCodes.Pop)
                {
                    flag = false;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return CodeInstruction.Call(typeof(PsycheHelper), nameof(PsycheHelper.TryGainMemoryReplacedPartBleedingHeart), new Type[] { typeof(Pawn), typeof(Pawn) });

                }
            }
        }



        //[HarmonyPrefix]
        //public static void BleedingHeartThought(Pawn pawn, Pawn billDoer)
        //{
        //    if (billDoer != null && billDoer.needs.mood != null)
        //        billDoer.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.ReplacedPartBleedingHeart, pawn);
        //}
    }
}
