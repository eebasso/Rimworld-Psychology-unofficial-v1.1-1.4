using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "BreakLoverAndFianceRelations")]
    public static class InteractionWorker_RomanceAttempt_BreakRelationsPatch
    {
        //[LogPerformance]
        [HarmonyPrefix]
        public static bool BreakRelations(Pawn pawn, ref List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            while (true)
            {
                Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                if (firstDirectRelationPawn != null && (!firstDirectRelationPawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) || !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous)))
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
                    Pawn recipient = firstDirectRelationPawn;
                    if (PsycheHelper.PsychologyEnabled(pawn) && PsycheHelper.PsychologyEnabled(recipient))
                    {
                        BreakupHelperMethods.AddExLover(pawn, recipient);
                        BreakupHelperMethods.AddExLover(recipient, pawn);
                        BreakupHelperMethods.AddBrokeUpOpinion(recipient, pawn);
                        BreakupHelperMethods.AddBrokeUpMood(recipient, pawn);
                        BreakupHelperMethods.AddBrokeUpMood(pawn, recipient);
                    }
                    else
                    {
                        pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
                    }
                    oldLoversAndFiances.Add(firstDirectRelationPawn);
                }
                else
                {
                    Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    if (firstDirectRelationPawn2 == null)
                    {
                        break;
                    }
                    else if (!firstDirectRelationPawn2.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) || !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                    {
                        pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
                        Pawn recipient2 = firstDirectRelationPawn2;
                        if (PsycheHelper.PsychologyEnabled(pawn) && PsycheHelper.PsychologyEnabled(recipient2))
                        {
                            BreakupHelperMethods.AddExLover(pawn, recipient2);
                            BreakupHelperMethods.AddExLover(recipient2, pawn);
                            BreakupHelperMethods.AddBrokeUpOpinion(recipient2, pawn);
                            BreakupHelperMethods.AddBrokeUpMood(recipient2, pawn);
                            BreakupHelperMethods.AddBrokeUpMood(pawn, recipient2);
                        }
                        else
                        {
                            pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
                        }
                        oldLoversAndFiances.Add(firstDirectRelationPawn2);
                    }
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.RandomSelectionWeight))]
    public static class InteractionWorker_RomanceAttempt_SelectionWeightPatch
    {
        //[LogPerformance]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void PsychologyException(ref float __result, Pawn initiator, Pawn recipient)
        {
            if (!PsycheHelper.PsychologyEnabled(initiator))
            {
                return;
            }
            // Disable during tutorial mode
            //if (TutorSystem.TutorialMode)
            //{
            //    __result = 0f;
            //    return;
            //}
            // Don't hit on yourself
            //if (initiator.def != recipient.def || initiator == recipient)
            //{
            //    __result = 0f;
            //    return;
            //}

            //Pawns won't hit on their partners.
            if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                __result = 0f;
                return;
            }
            //Codependents won't romance anyone if they are in a relationship
            if (LovePartnerRelationUtility.HasAnyLovePartner(initiator) && initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                __result = 0f;
                return;
            }

            /* GET INITIATOR PERSONALITY VALUES */
            float initiatorAggressive = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
            float initiatorExperimental = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
            float initiatorExtroverted = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted);
            float initiatorRomantic = PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
            float initiatorOpenMinded = initiator.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;
            bool initiatorLecher = initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher);

            //Don't hit on people in mental breaks... unless you're really freaky.
            //if (recipient.InMentalState && PsycheHelper.PsychologyEnabled(initiator) && initiatorExperimental < 0.8f)
            if (recipient.InMentalState && initiatorExperimental < 0.8f && !initiatorLecher)
            {
                __result = 0f;
                return;
            }

            /* ROMANCE CHANCE FACTOR INCLUDES THE FOLLOWING: */
            /* SEXUAL PREFERENCE FACTOR */
            /* AGE FACTOR */
            /* OTHER PAWN BEAUTY FACTOR */
            /* PAWN SEX AND ROMANCE DRIVE FACTORS */
            /* DISABILITY FACTOR */
            /* PSYCHIC LOVE SPELL FACTOR */
            float romanceChanceFactor = initiator.relations.SecondaryRomanceChanceFactor(recipient);
            if (romanceChanceFactor < 0.15f)
            {
                __result = 0f;
                return;
            }

            /* INITIATOR OPINION FACTOR */
            float initiatorOpinion = (float)initiator.relations.OpinionOf(recipient);
            float recipientOpinion = (float)recipient.relations.OpinionOf(initiator);
            float initiatorOpinionFactor = 1f;
            //Only lechers will romance someone that has less than base opinion of them
            if (!initiatorLecher)
            {
                if (initiatorOpinion < PsychologyBase.RomanceThreshold() || recipientOpinion < PsychologyBase.RomanceThreshold())
                {
                    __result = 0f;
                    return;
                }
                float minInitiatorOpinion = 30f * initiatorRomantic - 10f;
                initiatorOpinionFactor = Mathf.Max(1f, 2f * initiatorRomantic) * Mathf.InverseLerp(minInitiatorOpinion, 100f, initiatorOpinion);
            }

            /* INITIATOR EXISTING LOVE PARTNER FACTOR */
            //A pawn with high enough opinion of their lover will not hit on other pawns unless they are lecherous or polygamous (and their lover is also polygamous).
            float existingLovePartnerFactor = 1f;
            if (!new HistoryEvent(initiator.GetHistoryEventForLoveRelationCountPlusOne(), initiator.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
            {
                Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, allowDead: false);
                if (pawn != null && !initiatorLecher && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Polygamous) && !pawn.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
                {
                    float opinionOfLover = (float)initiator.relations.OpinionOf(pawn);
                    float maxOpinionOfLover = 75f - 50f * initiatorRomantic;
                    existingLovePartnerFactor = 0.5f * Mathf.Max(0, (maxOpinionOfLover - opinionOfLover) / maxOpinionOfLover);
                }
            }

            /* VANILLA HETERO WOMAN FACTOR */
            //float vanillaHeteroWomanFactor = 1f;
            //if (!PsycheHelper.PsychologyEnabled(initiator))
            //{
            //    //Vanilla: Straight women are 15% as likely to romance anyone.
            //    if (!initiator.story.traits.HasTrait(TraitDefOf.Gay) && !initiator.story.traits.HasTrait(TraitDefOf.Bisexual) && initiator.gender == Gender.Female)
            //    {
            //        vanillaHeteroWomanFactor = 0.15f;
            //    }
            //}

            /* INITIATOR PERSONALITY FACTOR */
            float personalityFactor = 1f;
            if (existingLovePartnerFactor >= 0.99f)
            {
                personalityFactor = Mathf.Pow(3f, initiatorAggressive + initiatorExtroverted - 1f);
            }

            /* INITIATOR KNOWN SEXUALITY FACTOR */
            //People who have hit on someone in the past and been rejected because of their sexuality will rarely attempt to hit on them again.
            //float knownSexualityFactor = (PsycheHelper.PsychologyEnabled(initiator) && PsychologyBase.ActivateKinsey() && PsycheHelper.Comp(initiator).Sexuality.IncompatibleSexualityKnown(recipient) && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher)) ? 0.05f : (PsycheHelper.PsychologyEnabled(initiator) ? (initiator.gender == recipient.gender ? (initiator.story.traits.HasTrait(TraitDefOf.Gay) && recipient.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : 0.15f) : (!initiator.story.traits.HasTrait(TraitDefOf.Gay) && !recipient.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : 0.15f)) : 1f);
            float knownSexualityFactor = (PsycheHelper.PsychologyEnabled(initiator) && PsychologyBase.ActivateKinsey() && PsycheHelper.Comp(initiator).Sexuality.IncompatibleSexualityKnown(recipient) && !initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher)) ? 0.05f : (PsycheHelper.PsychologyEnabled(initiator) ? (initiator.gender == recipient.gender ? (initiator.story.traits.HasTrait(TraitDefOf.Gay) && recipient.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : 0.15f) : (!initiator.story.traits.HasTrait(TraitDefOf.Gay) && !recipient.story.traits.HasTrait(TraitDefOf.Gay) ? 1f : 0.15f)) : 1f);

            /* INCEST FACTOR */
            float incestFactor = 1f;
            if (initiator.GetRelations(recipient).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                // Satanic black magic, sick shit
                incestFactor = Mathf.Pow(initiatorExperimental, 6.66f) + 0.666f * initiatorOpenMinded;
            }

            /* MULTIPLY ALL FACTORS TOGETHER */
            __result = PsychologyBase.RomanceChance() * romanceChanceFactor * initiatorOpinionFactor;
            __result *= existingLovePartnerFactor * personalityFactor * knownSexualityFactor * incestFactor;
            __result *= PsycheHelper.Comp(initiator).Sexuality.AdjustedRomanticDrive;
            return;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.Interacted))]
    public static class InteractionWorker_RomanceAttempt_InteractedLearnSexualityPatch
    {
        //[LogPerformance]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        public static bool LearnSexuality(Pawn initiator, Pawn recipient)
        {
            if (PsycheHelper.PsychologyEnabled(initiator) && PsycheHelper.PsychologyEnabled(recipient) && PsychologyBase.ActivateKinsey())
            {
                PsycheHelper.Comp(initiator).Sexuality.LearnSexuality(recipient);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "Interacted")]
    public static class InteractionWorker_RomanceAttempt_InteractedHandleThoughtsPatch
    {
        //[LogPerformance]
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
            }
            else if (extraSentencePacks.Contains(RulePackDefOf.Sentence_RomanceAttemptRejected))
            {
                if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
                {
                    initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.RebuffedMyRomanceAttemptLecher, recipient);
                }
            }
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), nameof(InteractionWorker_RomanceAttempt.SuccessChance))]
    public static class InteractionWorker_RomanceAttempt_SuccessChancePatch
    {
        //[LogPerformance]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void NewSuccessChance(ref float __result, Pawn initiator, Pawn recipient)
        {
            /* Throw out the result and replace it with our own formula. */
            if (!PsycheHelper.PsychologyEnabled(initiator))
            {
                return;
            }
            // Codependents won't romance anyone if they are in a relationship
            if (LovePartnerRelationUtility.HasAnyLovePartner(recipient) && recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                __result = 0f;
                return;
            }

            /* RECIPIENT PERSONALITY VALUES */
            //float recipientAggressive = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive);
            float recipientExperimental = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Experimental);
            float recipientExtroverted = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted);
            float recipientPure = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Pure);
            float recipientRomantic = PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Romantic);
            float recipientSexDrive = PsycheHelper.Comp(recipient).Sexuality.AdjustedSexDrive;
            float recipientRomanceDrive = PsycheHelper.Comp(recipient).Sexuality.AdjustedRomanticDrive;
            float recipientOpenMinded = recipient.story.traits.HasTrait(TraitDefOfPsychology.OpenMinded) ? 1f : 0f;
            bool recipientLecher = recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher);

            /* ROMANCE CHANCE FACTOR INCLUDES THE FOLLOWING: */
            /* SEXUAL PREFERENCE FACTOR */
            /* AGE FACTOR */
            /* OTHER PAWN BEAUTY FACTOR */
            /* PAWN SEX AND ROMANCE DRIVE FACTORS */
            /* DISABILITY FACTOR */
            /* PSYCHIC LOVE SPELL FACTOR */
            float romanceChanceFactor = recipient.relations.SecondaryRomanceChanceFactor(initiator);

            /* RECIPIENT OPINION FACTOR */
            float recipientOpinion = (float)recipient.relations.OpinionOf(initiator);
            float recipientOpinionFactor = 1f;
            if (!recipientLecher)
            {
                if (recipientOpinion < PsychologyBase.RomanceThreshold())
                {
                    __result = 0f;
                    return;
                }
                // More romantic recipients have higher standards but respond more strongly to overtures from high opinion initiators
                float minRecipientOpinion = 30f * recipientRomantic - 10f;
                recipientOpinionFactor = 2f * Mathf.Max(0.5f, recipientRomantic) * Mathf.InverseLerp(minRecipientOpinion, 100f, recipientOpinion);
            }

            /* EXISTING LOVE PARTNER FACTOR */
            float existingLovePartnerFactor = 1f;
            if (!recipient.story.traits.HasTrait(TraitDefOfPsychology.Polygamous))
            {
                Pawn pawn = null;
                if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0.01f : 0.6f;
                }
                else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0.01f : 0.1f;
                }
                else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                    existingLovePartnerFactor = (recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent)) ? 0.01f : 0.3f;
                }
                if (pawn != null)
                {
                    float opinionOfPartner = (float)recipient.relations.OpinionOf(pawn);
                    float loyalRecipientOpinion = Mathf.Min(150f - 100f * recipientRomantic, 100f);
                    float opinionOfPartnerFactor = Mathf.Min(opinionOfPartner / loyalRecipientOpinion, 1f);
                    existingLovePartnerFactor *= 1f - opinionOfPartnerFactor;

                    // Being romantically compatible will keep pawns faithful, but this weakens with lower opinion
                    existingLovePartnerFactor *= Mathf.Clamp01(1f - opinionOfPartnerFactor * Mathf.Clamp01(recipient.relations.SecondaryRomanceChanceFactor(pawn)));

                }
            }
            // Lechers won't care as much about their partners and are more promiscuous in general
            if (recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                existingLovePartnerFactor = 0.1f + Mathf.Sqrt(existingLovePartnerFactor);
            }

            /* INCEST FACTOR */
            float incestFactor = 1f;
            if (recipient.GetRelations(initiator).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                // Satanic black magic, sick shit
                incestFactor = Mathf.Pow(recipientExperimental, 6.66f) + 0.666f * recipientOpenMinded;
                incestFactor *= 2f * (1f - recipientPure);
                incestFactor = Mathf.Clamp01(incestFactor);
            }

            /* MULTIPLY FACTORS TOGETHER */
            float successChance = PsychologyBase.RomanceChance() * romanceChanceFactor * recipientOpinionFactor * existingLovePartnerFactor * incestFactor;
            // Stop rare events
            if (successChance < 0.01f)
            {
                successChance = 0f;
            }
            successChance /= 1f + successChance;
            //successChance = 1 - Mathf.Exp(-successChance);
            __result = Mathf.Clamp01(successChance);
        }
    }

    [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "TryAddCheaterThought")]
    public static class InteractionWorker_RomanceAttempt_CheaterThoughtPatch
    {
        //[LogPerformance]
        [HarmonyPostfix]
        public static void AddCodependentThought(Pawn pawn, Pawn cheater)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOfPsychology.CheatedOnMeCodependent, cheater);
        }
    }
}
