using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using UnityEngine;
using System.Diagnostics;

namespace Psychology;

public class InteractionWorker_Conversation : InteractionWorker
{
    
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (!PsycheHelper.PsychologyEnabled(initiator) || !PsycheHelper.PsychologyEnabled(recipient))
        {
            //Log.Message("InteractionWorker_Conversation.RandomSelectionWeight, not enabled for initiator " + initiator.LabelShort + ", recipient " + recipient.LabelShort);
            return 0f;
        }
        if (!initiator.health.capacities.CapableOf(PawnCapacityDefOf.Talking) || !recipient.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
        {
            //Log.Message("InteractionWorker_Conversation.RandomSelectionWeight, initiator " + initiator.LabelShort + ", recipient " + recipient.LabelShort + ", not talking");
            return 0f;
        }
        //float baseChance = 0.45f;
        //Lord lord = LordUtility.GetLord(initiator);
        //if (lord != null && (lord.LordJob is LordJob_HangOut || lord.LordJob is LordJob_Date) && LordUtility.GetLord(recipient) == lord)
        //{
        //    baseChance = 0.75f;
        //}
        //if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
        //{
        //    baseChance *= 1.2f;
        //}
        //return Mathf.Max(0f, baseChance + (PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly)-0.6f) + (PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted)-0.5f));
        float chanceFactor = 0.0f;
        Lord lord = LordUtility.GetLord(initiator);
        if (lord != null && (lord.LordJob is LordJob_HangOut || lord.LordJob is LordJob_Date) && LordUtility.GetLord(recipient) == lord)
        {
            chanceFactor += 0.5f;
        }
        if (initiator.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
        {
            chanceFactor += 0.5f;
        }
        chanceFactor += -0.5f + PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted);
        chanceFactor += -0.5f + PsycheHelper.Comp(initiator).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Outspoken);
        chanceFactor += -0.5f + PsycheHelper.Comp(recipient).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly);

        chanceFactor *= 4f;
        float multiplicativeFactor = chanceFactor > 0 ? 1f + chanceFactor : 1f / (1f - chanceFactor);
        //float multiplicativeFactor = chanceFactor > 0 ? Mathf.Sqrt(1f + 2f * chanceFactor) : 1f / (1f - chanceFactor);
        return 0.35f * multiplicativeFactor;
        //float successChance = 1f / (1f + Mathf.Pow(16f, -chanceFactor));
        //Log.Message("InteractionWorker_Conversation.RandomSelectionWeight, initiator " + initiator.LabelShort + ", recipient " + recipient.LabelShort + ", successChance " + successChance);
        //return successChance;
    }

    
    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
        PersonalityNode topic = (from node in PsycheHelper.Comp(initiator).Psyche.PersonalityNodes
                                 where node.HasConvoTopics
                                 select node).RandomElementByWeight(node => PsycheHelper.Comp(initiator).Psyche.GetConversationTopicWeight(node.def, recipient));
        string convoTopic = topic.def.conversationTopics.RandomElement();
        Hediff_Conversation initiatorHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, initiator);
        initiatorHediff.otherPawn = recipient;
        initiatorHediff.topic = topic.def;
        initiatorHediff.waveGoodbye = true;
        initiatorHediff.convoTopic = convoTopic;
        initiator.health.AddHediff(initiatorHediff);
        Hediff_Conversation recipientHediff = (Hediff_Conversation)HediffMaker.MakeHediff(HediffDefOfPsychology.HoldingConversation, recipient);
        recipientHediff.otherPawn = initiator;
        recipientHediff.topic = topic.def;
        recipientHediff.waveGoodbye = false;
        recipientHediff.convoTopic = convoTopic;
        recipient.health.AddHediff(recipientHediff);
    }

}
