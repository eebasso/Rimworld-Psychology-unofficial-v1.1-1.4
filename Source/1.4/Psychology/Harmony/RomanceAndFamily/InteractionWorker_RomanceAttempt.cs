﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection.Emit;
using System.Runtime;
using System.Security.Cryptography;


namespace Psychology.Harmony;

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
public static class InteractionWorker_RomanceAttempt_SelectionWeightPatch
{
  // Prefix or postfix?
  [HarmonyPriority(Priority.Last)]
  [HarmonyPostfix]
  public static void RandomSelectionWeightPrefix(ref float __result, Pawn initiator, Pawn recipient)
  {
    if (!SpeciesHelper.RomanceEnabled(initiator, true) || !SpeciesHelper.RomanceEnabled(recipient, true))
    {
      // No dating for non-teens, no exceptions
      __result = 0f;
      return;
      //return false;
    }

    // From vanilla, no romance in these cases
    if (TutorSystem.TutorialMode || LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
    {
      ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", already lovers");
      __result = 0f;
      return;
      //return false;
    }

    // Codependents won't romance anyone if they are in a relationship
    if (LovePartnerRelationUtility.HasAnyLovePartner(initiator) && initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
    {
      ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", codependent");
      __result = 0f;
      return;
      //return false;
    }

    //Don't hit on people in mental breaks... unless you're really freaky.
    float initiatorExperimental = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
    bool initiatorLecher = initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher);
    bool initiatorPsychopath = initiator.story.traits.HasTrait(TraitDefOf.Psychopath);

    if (recipient.InMentalState && initiatorExperimental < 0.8f && !initiatorLecher)
    {
      ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", mental state");
      __result = 0f;
      return;
      //return false;
    }
    /* ROMANCE CHANCE FACTOR INCLUDES THE FOLLOWING: */
    /* - SEXUAL PREFERENCE FACTOR */
    /* - AGE FACTOR */
    /* - OTHER PAWN BEAUTY FACTOR */
    /* - PAWN SEX AND ROMANCE DRIVE FACTORS */
    /* - INCEST FACTOR */
    /* - PSYCHIC LOVE SPELL FACTOR */

    float romChance = initiator.relations.SecondaryRomanceChanceFactor(recipient);
    ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", SecondaryRomanceChanceFactor = " + romChance);
    if (romChance < 0.15f)
    {
      ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", romChance < 0.15");
      __result = 0f;
      return;
      //return false;
    }
    //float romChanceMult = Mathf.InverseLerp(0.15f, 1f, romChance);
    float romChanceMult = (romChance - 0.15f) / 0.85f;

    // Assertive pawns are more likely to make a move on attractive mates
    float initiatorAggressive = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
    float initiatorConfident = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Confident);
    float chanceCutOff = 0.5f;
    float confidenceFactor = initiatorConfident + initiatorAggressive;
    float adjResult = romChanceMult < chanceCutOff ? romChanceMult : chanceCutOff + confidenceFactor * (romChanceMult - chanceCutOff);
    romChanceMult = adjResult;

    /* INITIATOR OPINION FACTOR */
    float initiatorRomantic = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
    float initiatorOpinion = (float)initiator.relations.OpinionOf(recipient);
    //float recipientOpinion = (float)recipient.relations.OpinionOf(initiator);
    float initiatorOpinMult;

    //Only lechers will romance someone that has less than base opinion of them
    if (!initiatorLecher)
    {
      //if (Mathf.Max(initiatorOpinion, recipientOpinion) < PsychologySettings.romanceOpinionThreshold)
      if (initiatorOpinion < PsychologySettings.romanceOpinionThreshold)
      {
        ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", initiator opinion too low");
        __result = 0f;
        return;
        //return false;
      }
      // Romantic pawns are more responsive to their opinion of the recipient
      float x = Mathf.InverseLerp(PsychologySettings.romanceOpinionThreshold, 100f, initiatorOpinion);
      initiatorOpinMult = 0.5f * Mathf.Pow(2f * x, 2f * initiatorRomantic + 1e-5f);
      if (initiatorPsychopath)
      {
        // Psychopaths have lower opinion standards
        initiatorOpinMult = 0.3f + 0.7f * initiatorOpinMult;
      }
    }
    else
    {
      // Lechers are more frisky
      initiatorOpinMult = 1f;
    }

    //float initiatorOpenMinded = initiator.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;

    /* INITIATOR EXISTING LOVE PARTNER FACTOR */
    //A pawn with high enough opinion of their lover will not hit on other pawns unless they are lecherous or polygamous (and their lover is also polygamous).
    float existingPartnerMult = 1f;
    if (!new HistoryEvent(initiator.GetHistoryEventForLoveRelationCountPlusOne(), initiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
    {
      Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
      if (pawn != null && !initiatorLecher && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)
          && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
      {
        float opinionOfLover = (float)initiator.relations.OpinionOf(pawn);
        // More romantic pawns will stay loyal longer, but will also turn faster against a partner they don't like
        float romDisp = 2f * initiatorRomantic - 1f;
        float maxOpinionOfLover = 50f - 40f * romDisp;
        float minOpinionOfLover = -50f + 25f * romDisp;
        existingPartnerMult = Mathf.InverseLerp(maxOpinionOfLover, minOpinionOfLover, opinionOfLover);
      }
    }

    /* INITIATOR KNOWN SEXUALITY FACTOR */
    float knownSexFactor;
    float straightWomanFactor;
    if (PsychologySettings.enableKinsey)
    {
      //People who have hit on someone in the past and been rejected because of their sexuality will rarely attempt to hit on them again.
      knownSexFactor = PsycheHelper.Comp(initiator).Sexuality.IncompatibleSexualityKnown(recipient) && !initiatorLecher ? 0.05f : 1f;
      // Not sure whether to keep this mechanic...
      // ToDo: make this a setting
      float kinseyFactor = PsycheHelper.Comp(initiator).Sexuality.kinseyRating / 6f;
      straightWomanFactor = initiator.gender == Gender.Female ? Mathf.Lerp(0.15f, 1f, kinseyFactor) : 1f;
    }
    else
    {
      bool initiatorIsGay = initiator.story.traits.HasTrait(TraitDefOf.Gay);
      bool recipientIsGay = recipient.story.traits.HasTrait(TraitDefOf.Gay);
      bool initiatorIsBisexual = initiator.story.traits.HasTrait(TraitDefOf.Bisexual);
      bool recipientIsBisexual = recipient.story.traits.HasTrait(TraitDefOf.Bisexual);

      if (initiator.gender == recipient.gender)
      {
        knownSexFactor = (initiatorIsGay || initiatorIsBisexual) && (recipientIsGay || recipientIsBisexual) ? 1f : 0.15f;
      }
      else
      {
        knownSexFactor = (!initiatorIsGay || initiatorIsBisexual) && (!recipientIsGay || recipientIsBisexual) ? 1f : 0.15f;
      }
      // ToDo: make this a setting
      straightWomanFactor = initiatorIsGay ? 1f : initiator.gender == Gender.Female ? 0.15f : 1f;
    }

    // Include chance multiplier from settings
    __result = 1.15f * PsychologySettings.romanceChanceMultiplier * romChanceMult * initiatorOpinMult * existingPartnerMult * knownSexFactor * straightWomanFactor;

    //float initiatorAggressive = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
    //float initiatorConfident = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Confident);
    //float chanceCutOff = 0.5f;
    //float confidenceFactor = initiatorConfident + initiatorAggressive;
    //float adjResult = __result < chanceCutOff ? __result : chanceCutOff + confidenceFactor * (__result - chanceCutOff);
    //__result = adjResult;

    ////Log.Message("InteractionWorker_RomanceAttempt.RandomSelectionWeight, initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", romChanceMult = " + romChanceMult + ", initiatorOpinMult = " + initiatorOpinMult + ", existingPartnerMult = " + existingPartnerMult + ", knownSexFactor = " + knownSexFactor + ", straightWomanFactor = " + straightWomanFactor + ", result = " + __result);

    //return false;
  }

  [HarmonyPostfix]
  [HarmonyPriority(Priority.Last)]
  public static void RandomSelectionWeightPostfix(ref float __result, Pawn initiator, Pawn recipient)
  {
    // Just to be extra sure, add a postfix lifestage check
    if (!SpeciesHelper.RomanceEnabled(initiator, true) || !SpeciesHelper.RomanceEnabled(recipient, true))
    {
      // No dating for non-teens, no exceptions
      __result = 0f;
    }
  }


}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
public static class InteractionWorker_RomanceAttempt_SuccessChancePatch
{
  [HarmonyPrefix]
  public static bool SuccessChancePrefix(Pawn initiator, Pawn recipient, float baseChance, ref float __result)
  {
    //Log.Warning("InteractionWorker_RomanceAttempt.SuccessChance fired!");

    /* Throw out the result and replace it with our own formula. */
    if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
    {
      __result = 0f;
      return false;
    }
    if (!SpeciesHelper.RomanceEnabled(initiator, true) || !SpeciesHelper.RomanceEnabled(recipient, true))
    {
      // No underage dating
      __result = 0f;
      return false;
    }
    if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
    {
      // Codependents won't romance anyone if they are in a relationship
      if (LovePartnerRelationUtility.HasAnyLovePartner(recipient))
      {
        __result = 0f;
        return false;
      }
      // Codependents will always get back together with their ex if they don't have a love partner
      if (LovePartnerRelationUtility.ExLovePartnerRelationExists(initiator, recipient))
      {
        __result = 1f;
        return false;
      }
    }
    return true;
  }

  [HarmonyPostfix]
  public static void SuccessChancePostfix(Pawn initiator, Pawn recipient, ref float __result)
  {
    if (!SpeciesHelper.RomanceEnabled(initiator, true) || !SpeciesHelper.RomanceEnabled(recipient, true))
    {
      // No underage dating
      __result = 0f;
    }
    if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
    {
      // Codependents won't romance anyone if they are in a relationship
      if (LovePartnerRelationUtility.HasAnyLovePartner(recipient))
      {
        __result = 0f;
      }
      // Codependents will always get back together with their ex if they don't have a love partner
      if (LovePartnerRelationUtility.ExLovePartnerRelationExists(initiator, recipient))
      {
        __result = 1f;
      }
    }
    __result *= PsychologySettings.romanceChanceMultiplier;
    // Always prevent 100% chance, not sure whether to do this
    __result = 1f - Mathf.Exp(-__result);
  }


  //[HarmonyPriority(Priority.Last)]
  //[HarmonyPrefix]
  //public static bool SuccessChancePrefix(ref float __result, Pawn initiator, Pawn recipient)
  //{
  //    //Log.Warning("InteractionWorker_RomanceAttempt.SuccessChance fired!");

  //    /* Throw out the result and replace it with our own formula. */
  //    if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
  //    {
  //        __result = 0f;
  //        return false;
  //    }
  //    if (!SpeciesHelper.RomanceLifestageAgeCheck(initiator, true) || !SpeciesHelper.RomanceLifestageAgeCheck(recipient, true))
  //    {
  //        // No underage dating
  //        __result = 0f;
  //        return false;
  //    }
  //    bool recipientCodependent = recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent);
  //    if (recipientCodependent)
  //    {
  //        // Codependents won't romance anyone if they are in a relationship
  //        if (LovePartnerRelationUtility.HasAnyLovePartner(recipient))
  //        {
  //            __result = 0f;
  //            return false;
  //        }
  //        // Codependents will always get back together with their ex if they don't have a love partner
  //        if (LovePartnerRelationUtility.ExLovePartnerRelationExists(initiator, recipient))
  //        {
  //            __result = 1f;
  //            return false;
  //        }
  //    }

  //    /* ROMANCE CHANCE FACTOR INCLUDES THE FOLLOWING: */
  //    /* SEXUAL PREFERENCE FACTOR */
  //    /* AGE FACTOR */
  //    /* OTHER PAWN BEAUTY FACTOR */
  //    /* PAWN SEX AND ROMANCE DRIVE FACTORS */
  //    /* DISABILITY FACTOR */
  //    /* PSYCHIC LOVE SPELL FACTOR */
  //    bool recipientLecher = recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher);
  //    float romChanceFactor = recipient.relations.SecondaryRomanceChanceFactor(initiator);
  //    if (romChanceFactor < 0.15f && !recipientLecher)
  //    {
  //        __result = 0f;
  //        return false;
  //    }

  //    /* RECIPIENT OPINION FACTOR */
  //    float recipientRomantic = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
  //    bool recipientPsychopath = recipient.story.traits.HasTrait(TraitDefOf.Psychopath);
  //    float recipientOpinion = (float)recipient.relations.OpinionOf(initiator);
  //    if (recipientOpinion < PsychologySettings.romanceOpinionThreshold && !recipientLecher)
  //    {
  //        __result = 0f;
  //        return false;
  //    }
  //    float opinionFactor = Mathf.InverseLerp(PsychologySettings.romanceOpinionThreshold, 100f, recipientOpinion);
  //    if (recipientLecher)
  //    {
  //        // Lechers will romance someone that they have a low opinion of
  //        opinionFactor = 0.5f + Mathf.Sqrt(opinionFactor);
  //    }
  //    if (recipientPsychopath)
  //    {
  //        // Psychopaths have lower opinion standards
  //        opinionFactor = 0.3f + 0.7f * opinionFactor;
  //    }
  //    // More romantic recipients have higher standards but respond more strongly to overtures from high opinion initiators
  //    float recipientOpinionFactor = 0.5f * Mathf.Pow(2f * opinionFactor, 2f * recipientRomantic + 1e-5f);

  //    float existingLovePartnerMult = 1f;
  //    /* EXISTING LOVE PARTNER FACTOR */
  //    if (!new HistoryEvent(recipient.GetHistoryEventForLoveRelationCountPlusOne(), recipient.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo() && !recipient.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
  //    {
  //        Pawn pawn = null;
  //        if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
  //        {
  //            pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
  //            //existingLovePartnerMult = recipientCodependent ? 0.01f : 0.6f;
  //            existingLovePartnerMult = 0.6f;
  //        }
  //        else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
  //        {
  //            pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
  //            //existingLovePartnerMult = recipientCodependent ? 0.01f : 0.1f;
  //            existingLovePartnerMult = 0.1f;
  //        }
  //        else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, (Pawn x) => !x.Dead) != null)
  //        {
  //            pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
  //            //existingLovePartnerMult = recipientCodependent ? 0.01f : 0.3f;
  //            existingLovePartnerMult = 0.3f;
  //        }
  //        if (pawn != null)
  //        {
  //            float opinionOfPartner = (float)recipient.relations.OpinionOf(pawn);
  //            float romDisp = 2f * recipientRomantic - 1f;
  //            // More romantic pawns stay loyal for longer
  //            float opinionOfPartnerMult = romDisp > 0 ? Mathf.InverseLerp(100f - 75f * romDisp, 0, opinionOfPartner) : Mathf.InverseLerp(100f, 75f * romDisp, opinionOfPartner);
  //            float romChanceMult = Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
  //            // Modified the formula so that opinion still matters even with maximum romance chance factor
  //            // Being romantically compatible will keep pawns more faithful, but this weakens with lower opinion
  //            existingLovePartnerMult *= 0.5f * (opinionOfPartnerMult + romChanceMult);
  //        }
  //        // Lechers won't care as much about their partners and are more promiscuous in general
  //        if (recipientLecher)
  //        {
  //            existingLovePartnerMult = 0.5f + Mathf.Sqrt(existingLovePartnerMult);
  //        }
  //    }

  //    // Account for user setting of romance chance multiplier
  //    __result = 0.6f * PsychologySettings.romanceChanceMultiplier * existingLovePartnerMult * recipientOpinionFactor * romChanceFactor;
  //    // Always prevent 100% chance, not sure whether to do this
  //    __result = 1f - Mathf.Exp(-__result);

  //    ////Log.Message("InteractionWorker_RomanceAttempt.SuccessChance initiator = " + initiator.LabelShort + ", recipient = " + recipient.LabelShort + ", romChanceFactor = " + romChanceFactor + ", recipientOpinionFactor = " + recipientOpinionFactor + ", existingLovePartnerMult = " + existingLovePartnerMult + ", successChance = " + __result);

  //    return false;
  //}
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "OpinionFactor")]
public static class InteractionWorker_RomanceAttempt_OpinionFactor_Patches
{
  [HarmonyTranspiler]
  public static IEnumerable<CodeInstruction> OpinionFactorTranspiler(IEnumerable<CodeInstruction> codes)
  {
    return RomancePatchUtility.ChangeMinRomanceOpinion(codes);
  }

  [HarmonyPostfix]
  public static void OpinionFactorPostfix(Pawn initiator, Pawn recipient, ref float __result)
  {
    if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
    {
      // Lechers will romance someone that they have a low opinion of
      __result = 0.5f + Mathf.Sqrt(__result);
    }
    if (recipient.story.traits.HasTrait(TraitDefOf.Psychopath))
    {
      // Psychopaths have lower opinion standards
      __result = 0.3f + 0.7f * __result;
    }
  }

}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "PartnerFactor")]
public static class InteractionWorker_RomanceAttempt_PartnerFactor_Patches
{
  public static Pawn pawn;

  [HarmonyPrefix]
  public static bool PartnerFactorPrefix(Pawn initiator, Pawn recipient, ref float __result)
  {
    if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(initiator))
    {
      __result = 0f;
      return false;
    }

    if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
    {
      __result = 1f;
      return false;
    }

    float recipientRomantic = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);

    float existingLovePartnerFactor = 1f;
    if (!new HistoryEvent(recipient.GetHistoryEventForLoveRelationCountPlusOne(), recipient.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
    {
      Pawn pawn = null;
      if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
      {
        pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
        existingLovePartnerFactor = 0.6f;
      }
      else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
      {
        pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
        existingLovePartnerFactor = 0.1f;
      }
      else if (recipient.GetSpouseCount(includeDead: false) > 0)
      {
        pawn = recipient.GetMostLikedSpouseRelation().otherPawn;
        existingLovePartnerFactor = 0.3f;
      }
      if (pawn != null)
      {
        //num *= Mathf.InverseLerp(100f, 0f, recipient.relations.OpinionOf(pawn));
        //num *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
        float opinionOfPartner = (float)recipient.relations.OpinionOf(pawn);
        float romDisp = 2f * recipientRomantic - 1f;
        // More romantic pawns stay loyal for longer
        float opinionOfPartnerMult = romDisp > 0 ? Mathf.InverseLerp(100f - 75f * romDisp, 0, opinionOfPartner) : Mathf.InverseLerp(100f, 75f * romDisp, opinionOfPartner);
        float romChanceMult = Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
        // Modified the formula so that opinion still matters even with maximum romance chance factor
        // Being romantically compatible will keep pawns more faithful, but this weakens with lower opinion
        existingLovePartnerFactor *= 0.5f * (opinionOfPartnerMult + romChanceMult);
      }
      // Lechers won't care as much about their partners and are more promiscuous in general
      if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
      {
        existingLovePartnerFactor = 0.5f + Mathf.Sqrt(existingLovePartnerFactor);
      }

    }
    __result = existingLovePartnerFactor;
    return false;
  }
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
public static class InteractionWorker_RomanceAttempt_Interacted_Patches
{
  [HarmonyPriority(Priority.High)]
  [HarmonyPrefix]
  public static bool LearnSexuality(Pawn initiator, Pawn recipient)
  {
    if (PsycheHelper.PsychologyEnabled(initiator) && PsycheHelper.PsychologyEnabled(recipient) && PsychologySettings.enableKinsey)
    {
      PsycheHelper.Comp(initiator).Sexuality.LearnSexuality(recipient);
    }
    return true;
  }

  [HarmonyPostfix]
  public static void HandleNewThoughts(InteractionWorker_RomanceAttempt __instance, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, string letterText, string letterLabel, LetterDef letterDef)
  {
    if (extraSentencePacks.Contains(RulePackDefOf.Sentence_RomanceAttemptAccepted))
    {
      foreach (ThoughtDef d in (from tgt in initiator.needs.mood.thoughts.memories.Memories
                                where tgt.def.defName.Contains("BrokeUpWithMe")
                                select tgt.def))
      {
        initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(d, recipient);
      }
      foreach (ThoughtDef d in (from tgt in recipient.needs.mood.thoughts.memories.Memories
                                where tgt.def.defName.Contains("BrokeUpWithMe")
                                select tgt.def))
      {
        recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(d, initiator);
      }
      initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, recipient);
      recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
      return;
    }
    if (extraSentencePacks.Contains(RulePackDefOf.Sentence_RomanceAttemptRejected))
    {
      if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
      {
        initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RebuffedMyRomanceAttemptLecher, recipient);
      }
      return;
    }
  }

}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "BreakLoverAndFianceRelations")]
public static class InteractionWorker_RomanceAttempt_BreakLoverAndFianceRelationsPatch
{

  public static bool Prefix(Pawn pawn, ref List<Pawn> oldLoversAndFiances)
  {
    oldLoversAndFiances = new List<Pawn>();
    return pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) != true;
  }

  [HarmonyTranspiler]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
  {
    List<CodeInstruction> cList = codes.ToList();
    for (int i = 0; i < cList.Count(); i++)
    {
      if (cList[i].opcode == OpCodes.Brfalse_S)
      {
        yield return CodeInstruction.Call(typeof(InteractionWorker_RomanceAttempt_BreakLoverAndFianceRelationsPatch), nameof(MakeNullIfPolygamous));
      }
      yield return cList[i];
    }
  }

  public static Pawn MakeNullIfPolygamous(Pawn pawn)
  {
    if (pawn == null)
    {
      return null;
    }
    if (pawn.story?.traits?.HasTrait(TraitDefOfPsychology.Polygamous) == true)
    {
      return null;
    }
    return pawn;
  }
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "TryAddCheaterThought")]
public static class InteractionWorker_RomanceAttempt_TryAddCheaterThoughtPatch
{
  [HarmonyTranspiler]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
  {
    return RomanceHelperMethods.InterdictTryGainAndRemoveMemories(codes);
  }
}

[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RemoveBrokeUpAndFailedRomanceThoughts")]
public static class InteractionWorker_RomanceAttempt_RemoveBrokeUpAndFailedRomanceThoughtsPatch
{
  [HarmonyTranspiler]
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
  {
    return RomanceHelperMethods.InterdictTryGainAndRemoveMemories(codes);
  }

  //[HarmonyPostfix]
  //public static void RemoveBrokeUpAndFailedRomanceThoughts(Pawn pawn, Pawn otherPawn)
  //{
  //    if (pawn.needs.mood != null)
  //    {
  //        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(BreakupHelperMethods.BrokeUpWithMeMoodDef, otherPawn);
  //        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.IBrokeUpWithThemMood, otherPawn);
  //        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, otherPawn);
  //    }
  //}
}

//[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "TryAddCheaterThought")]
//public static class InteractionWorker_RomanceAttempt_CheaterThoughtPatch
//{
//    [HarmonyPostfix]
//    public static void AddCodependentThought(Pawn pawn, Pawn cheater)
//    {
//        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CheatedOnMeCodependent, cheater);
//    }
//}

