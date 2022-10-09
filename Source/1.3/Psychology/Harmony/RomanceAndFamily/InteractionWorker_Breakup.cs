using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Psychology.Harmony;

[HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.RandomSelectionWeight), new[] { typeof(Pawn), typeof(Pawn) })]
public static class InteractionWorker_RandomSelectionWeight_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(InteractionWorker_Breakup __instance, ref float __result, Pawn initiator, Pawn recipient)
    {
        if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
        {
            __result = 0f;
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    public static void Postfix(InteractionWorker_Breakup __instance, ref float __result, Pawn initiator, Pawn recipient)
    {
        if (PsycheHelper.PsychologyEnabled(initiator))
        {
            __result *= Mathf.Lerp(2f, 0f, PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
        }
    }

    //[HarmonyPrefix]
    //public static bool NewSelectionWeight(InteractionWorker_Breakup __instance, ref float __result, Pawn initiator, Pawn recipient)
    //{
    //    /* Also this one. */
    //    if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
    //    {
    //        __result = 0f;
    //        return false;
    //    }
    //    else if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
    //    {
    //        __result = 0f;
    //        return false;
    //    }
    //    float chance = 0.02f;
    //    float romanticFactor = 1f;
    //    if (PsycheHelper.PsychologyEnabled(initiator))
    //    {
    //        chance = 0.05f;
    //        romanticFactor = Mathf.InverseLerp(1.05f, 0f, PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
    //    }
    //    float opinionFactor = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
    //    float spouseFactor = 1f;
    //    if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
    //    {
    //        spouseFactor = 0.4f;
    //    }
    //    __result = chance * romanticFactor * opinionFactor * spouseFactor;
    //    return false;
    //}

}

[HarmonyPatch(typeof(InteractionWorker_Breakup), nameof(InteractionWorker_Breakup.Interacted))]
public static class InteractionWorker_Breakup_Interacted_Patch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        return BreakupHelperMethods.InterdictTryGainAndRemoveMemories(codes);
    }
    //[HarmonyTranspiler]
    //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    //{
    //    FieldInfo fieldInfoMemories = AccessTools.Field(typeof(ThoughtHandler), nameof(ThoughtHandler.memories));
    //    FieldInfo fieldInfoBrokeUpWithMe = AccessTools.Field(typeof(ThoughtDefOf), nameof(ThoughtDefOf.BrokeUpWithMe));
    //    MethodInfo methodInfoTryGainMemory = AccessTools.Method(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory));

    //    bool success = false;
    //    bool foundMemories = false;
    //    bool foundBrokeUpWithMe = false;
    //    bool foundTryGainMemory = false;
    //    int indexMemories = -1;
    //    int indexBrokeUpWithMe = -1;
    //    int indexTryGainMemory = -1;
    //    List<CodeInstruction> clist = codes.ToList();
    //    for (int i = 0; i < clist.Count(); i++)
    //    {
    //        if (clist[i].LoadsField(fieldInfoMemories))
    //        {
    //            foundMemories = true;
    //            indexMemories = i;
    //        }
    //        else if (clist[i].LoadsField(fieldInfoBrokeUpWithMe))
    //        {
    //            foundBrokeUpWithMe = true;
    //            indexBrokeUpWithMe = i;
    //        }
    //        else if (clist[i].Calls(methodInfoTryGainMemory))
    //        {
    //            foundTryGainMemory = true;
    //            indexTryGainMemory = i;
    //        }
    //    }
    //    success = foundMemories && foundBrokeUpWithMe && foundTryGainMemory && indexMemories < indexBrokeUpWithMe && indexBrokeUpWithMe < indexTryGainMemory;
    //    if (success != true)
    //    {
    //        Log.Error("InteractionWorker_Breakup_Interacted_Patch.Transpiler failed");
    //        foreach (CodeInstruction c in codes) yield return c;
    //        yield break;
    //    }
    //    int startIndex = indexMemories - 5;
    //    for (int i = 0; i < clist.Count(); i++)
    //    {
    //        yield return clist[i];
    //        if (i == startIndex)
    //        {
    //            yield return new CodeInstruction(OpCodes.Ldarg_1);
    //            yield return new CodeInstruction(OpCodes.Ldarg_2);
    //            yield return CodeInstruction.Call(typeof(InteractionWorker_Breakup_Interacted_Patch), nameof(BrokeUpWithMePsychology));

    //            i += indexTryGainMemory - startIndex;
    //        }


    //    }
    //}

    // Could delete thought to make, and make it manually with custom mood offset
    //public static void BrokeUpWithMePsychology(Pawn initiator, Pawn recipient)
    //{
    //    Thought_MemorySocial thought = ThoughtMaker.MakeThought(ThoughtDefOf.BrokeUpWithMe) as Thought_MemorySocial;
    //    thought.opinionOffset *= PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic) * Mathf.InverseLerp(5f, 100f, recipient.relations.OpinionOf(initiator));
    //    thought.opinionOffset = Mathf.Ceil(thought.opinionOffset);
    //    recipient.needs.mood.thoughts.memories.TryGainMemory(thought, initiator);
    //}


    //[HarmonyPrefix]
    //public static void Prefix(InteractionWorker_Breakup __instance, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
    //{
    //    recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);

    //    if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient) != true && PsycheHelper.PsychologyEnabled(initiator) && PsycheHelper.PsychologyEnabled(recipient))
    //    {
    //        BreakupHelperMethods.AddBrokeUpOpinion(recipient, initiator);
    //        BreakupHelperMethods.AddBrokeUpMood(recipient, initiator);
    //        BreakupHelperMethods.AddBrokeUpMood(initiator, recipient);
    //    }
    //}
}

