using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(MentalState_SocialFighting), nameof(MentalState_SocialFighting.PostEnd))]
public static class MentalState_SocialFighting_PostEndPatches
{
    [HarmonyPostfix]
    public static void WhoWon(MentalState_SocialFighting __instance, Pawn ___pawn, Pawn ___otherPawn)
    {
        float damage = ___pawn.health.summaryHealth.SummaryHealthPercent - ___otherPawn.health.summaryHealth.SummaryHealthPercent;
        if (damage > 0.05f)
        {
            ___pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.WonFight, ___otherPawn);
        }
        else if (damage < -0.05f)
        {
            ___otherPawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.WonFight, ___pawn);
        }
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddPersonalityHook(IEnumerable<CodeInstruction> instrs)
    {
        float num;
        bool success = false;
        foreach (CodeInstruction itr in instrs)
        {
            if (itr.opcode == OpCodes.Ldc_R4 && float.TryParse("" + itr.operand, out num) && num == 0.5f)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MentalState_SocialFighting), "pawn"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MentalState_SocialFighting_PostEndPatches), nameof(MentalState_SocialFighting_PostEndPatches.PersonalityChance), new Type[] { typeof(Pawn) }));
                success = true;
            }
            else
            {
                yield return itr;
            }
        }
        if (success != true)
        {
            Log.Error("Psychology: patch of MentalState_SocialFighting.PostEnd failed");
        }
    }

    public static float PersonalityChance(Pawn pawn)
    {
        if (PsycheHelper.PsychologyEnabled(pawn))
        {
            return 1f - PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
        }
        return 0.5f;
    }
}
