using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace Psychology.Harmony;

//[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.RandomSelectionWeight), new[] { typeof(Pawn), typeof(Pawn) })]
//public static class InteractionWorker_MarriageProposal_SelectionWeightPatch
//{
//    [HarmonyPostfix]
//    [HarmonyPriority(Priority.Last)]
//    public static void _RandomSelectionWeight(InteractionWorker_MarriageProposal __instance, Pawn initiator, Pawn recipient, ref float __result)
//    {
//        if (__result == 0f)
//        {
//            return;
//        }
//        if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
//        {
//            __result = 0f;
//            return;
//        }
//        if (initiator.relations.GetDirectRelation(PawnRelationDefOf.Lover, recipient) == null)
//        {
//            __result = 0f;
//            return;
//        }
//        if (!SpeciesHelper.RomanceLifestageAgeCheck(initiator, false) || !SpeciesHelper.RomanceLifestageAgeCheck(recipient, false))
//        {
//            __result = 0f;
//            return;
//        }
//        CompPsychology initiatorComp = PsycheHelper.Comp(initiator);
//        __result *= initiatorComp.Psyche.GetPersonalityRating(PersonalityNodeDefOf.Adventurous) + initiatorComp.Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
//        if (PsychologySettings.enableKinsey)
//        {
//            if (initiator.gender == Gender.Female)
//            {
//                __result /= 0.2f; // Undo vanilla effect
//                __result *= Mathf.Lerp(0.2f, 1f, initiatorComp.Sexuality.kinseyRating / 3f);
//            }
//            __result *= 1.2f * Mathf.Sqrt(initiatorComp.Sexuality.AdjustedRomanticDrive);
//        }
//        if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
//        {
//            __result *= 2f;
//        }
//        __result *= PsychologySettings.romanceChanceMultiplier;
//    }
//}

[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.AcceptanceChance))]
public static class InteractionWorker_MarriageProposal_AcceptanceChancePatch
{
    [HarmonyPostfix]
    public static void PsychologyException(InteractionWorker_MarriageProposal __instance, Pawn initiator, Pawn recipient, ref float __result)
    {
        if (PsycheHelper.PsychologyEnabled(recipient) != true)
        {
            __result = 0f;
            return;
        }
        if (!SpeciesHelper.RomanceEnabled(initiator, false) || !SpeciesHelper.RomanceEnabled(recipient, false))
        {
            __result = 0f;
            return;
        }
        if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
        {
            // Codependent pawns will always accept a marriage proposal
            __result = 1f;
            return;
        }
        
        CompPsychology recipientComp = PsycheHelper.Comp(recipient);
        float recipientRomanatic = recipientComp.Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
        float recipientPure = recipientComp.Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);

        float x = Mathf.Clamp(-1f + recipientRomanatic + recipientPure, -0.999f, 0.999f);
        x *= 0.8f;
        float num = Mathf.Clamp(-1f + 2f * __result, -0.999f, 0.999f);
        num = PsycheHelper.RelativisticAddition(num, x);
        __result = 0.5f * (1f + num);

        if (PsychologySettings.enableKinsey)
        {
            num *= 1.2f * Mathf.Sqrt(recipientComp.Sexuality.AdjustedRomanticDrive);
        }
        __result = Mathf.Clamp01(__result);
    }

    //[HarmonyPrefix]
    //public static bool PsychologyException(InteractionWorker_MarriageProposal __instance, ref float __result, Pawn initiator, Pawn recipient)
    //{
    //    if (PsycheHelper.PsychologyEnabled(recipient))
    //    {
    //        float num = 1.2f;
    //        num *= Mathf.InverseLerp(0f, 0.75f, PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic));
    //        if (PsychologySettings.enableKinsey)
    //        {
    //            num *= PsycheHelper.Comp(recipient).Sexuality.AdjustedRomanticDrive;
    //        }
    //        num *= Mathf.Clamp01(GenMath.LerpDouble(-20f, 60f, 0f, 1f, (float)recipient.relations.OpinionOf(initiator)));
    //        __result = Mathf.Clamp01(num);
    //        return false;
    //        /* If the recipient is a PsychologyPawn, the mod takes over AcceptanceChance for them and the normal method will be ignored. */
    //    }
    //    return true;
    //}
}

[HarmonyPatch(typeof(InteractionWorker_MarriageProposal), nameof(InteractionWorker_MarriageProposal.Interacted))]
public static class InteractionWorker_MarriageProposal_InteractedPatch
{
    // TryGain RejectedMyProposal
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
    {
        return RomanceHelperMethods.InterdictTryGainAndRemoveMemories(codes);
    }
}


