using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Verse.Grammar;
using System.Reflection;
using HarmonyLib;

namespace Psychology;

public class Hediff_Conversation : HediffWithComps
{
    public Pawn otherPawn;
    public PersonalityNodeDef topic;
    public string convoTopic;
    public bool waveGoodbye;
    public bool startedFight = false;
    public PlayLogEntry_InteractionConversation convoLog;

    public override void PostMake()
    {
        base.PostMake();
        if (!PsycheHelper.PsychologyEnabled(this.pawn))
        {
            this.pawn.health.RemoveHediff(this);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref this.otherPawn, "otherPawn");
        Scribe_Defs.Look(ref this.topic, "topic");
        Scribe_Values.Look(ref this.waveGoodbye, "waveGoodbye");
        Scribe_Values.Look(ref this.convoTopic, "convoTopic", "something");
    }

    public override void Tick()
    {
        base.Tick();
        if (this.otherPawn == null)
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (!this.otherPawn.Spawned || !this.pawn.Spawned || !InteractionUtility.CanReceiveInteraction(this.pawn) || !InteractionUtility.CanReceiveInteraction(this.otherPawn))
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (this.pawn.Map != null && this.otherPawn.Map != null && ((this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true)))
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if ((this.pawn.Position - this.otherPawn.Position).LengthHorizontalSquared >= 54f || !GenSight.LineOfSight(this.pawn.Position, this.otherPawn.Position, this.pawn.Map, true))
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }

        if (this.otherPawn.Dead || this.otherPawn.Downed || this.otherPawn.InAggroMentalState)
        {
            this.pawn.health.RemoveHediff(this);
            return;
        }
        if (this.pawn.IsHashIntervalTick(200))
        {
            /* When a conversation first starts, the mean time for it to last is 3 hours.
             * When it reaches half an hour, the mean time for it to continue is 2 hours.
             * When it reaches an hour, the mean time for it to continue is 1 hour.
             * When it surpasses 2 hours, it will on average last for half an hour more.
             * Conversations will thus usually not surpass 2 hours, and very rarely surpass 2 and a half hours, but are very likely to last up to an hour.
             */
            float mtb = 3f;
            if (this.ageTicks > 2f * GenDate.TicksPerHour)
            {
                mtb = 0.5f;
            }
            else if (this.ageTicks > GenDate.TicksPerHour)
            {
                mtb = 1f;
            }
            else if (this.ageTicks > 0.5f * GenDate.TicksPerHour)
            {
                mtb = 2f;
            }
            if (pawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
            {
                mtb *= 2f;
            }
            if (this.otherPawn.story.traits.HasTrait(TraitDefOfPsychology.Chatty))
            {
                mtb *= 2f;
            }
            if (Rand.MTBEventOccurs(mtb, GenDate.TicksPerHour, 200))
            {
                this.pawn.health.RemoveHediff(this);
                return;
            }
            else if (Rand.Value < 0.2f && this.pawn.Map != null)
            {
                MoteMaker.MakeInteractionBubble(this.pawn, otherPawn, InteractionDefOfPsychology.EndConversation.interactionMote, InteractionDefOfPsychology.EndConversation.GetSymbol()); // TEST in 1.3
            }
        }
    }

    public override void PostRemoved()
    {
        
        base.PostRemoved();
        if (this.pawn == null && this.otherPawn == null)
        {
            Log.Message("Hediff_Conversation.PostRemoved(), pawns were null");
            return;
        }
        if (this.pawn.Dead || this.otherPawn.Dead || !PsycheHelper.PsychologyEnabled(pawn) || !PsycheHelper.PsychologyEnabled(otherPawn))
        {
            Log.Message("Hediff_Conversation.PostRemoved(), psychology not enabled");
            return;
        }
        //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 0");
        Hediff_Conversation otherConvo = otherPawn.health.hediffSet.hediffs.Find(h => h is Hediff_Conversation && ((Hediff_Conversation)h).otherPawn == this.pawn) as Hediff_Conversation;
        if (otherConvo != null)
        {
            this.otherPawn.health.RemoveHediff(otherConvo);
            this.startedFight = otherConvo.startedFight;
        }
        string talkDesc;
        if (this.ageTicks < 500)
        {
            int numShortTalks = int.Parse("NumberOfShortTalks".Translate());
            talkDesc = "ShortTalk" + Rand.RangeInclusive(1, numShortTalks);
        }
        else if (this.ageTicks < GenDate.TicksPerHour / 2)
        {
            int numNormalTalks = int.Parse("NumberOfNormalTalks".Translate());
            talkDesc = "NormalTalk" + Rand.RangeInclusive(1, numNormalTalks);
        }
        else if (this.ageTicks < GenDate.TicksPerHour * 2.5)
        {
            int numLongTalks = int.Parse("NumberOfLongTalks".Translate());
            talkDesc = "LongTalk" + Rand.RangeInclusive(1, numLongTalks);
        }
        else
        {
            int numEpicTalks = int.Parse("NumberOfEpicTalks".Translate());
            talkDesc = "EpicTalk" + Rand.RangeInclusive(1, numEpicTalks);
        }
        //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 1");
        float opinionMod;
        ThoughtDef def = CreateSocialThought(out opinionMod);
        bool mattered = TryGainThought(def, Mathf.RoundToInt(opinionMod));
        InteractionDef endConversation = new InteractionDef();
        endConversation.socialFightBaseChance = 0.2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive) * PopulationModifier * Mathf.InverseLerp(0f, -80f, opinionMod);
        endConversation.defName = "EndConversation";
        endConversation.label = def.label;
        List<RulePackDef> socialFightPacks = new List<RulePackDef>();
        if (otherConvo != null && (startedFight || (mattered && this.pawn.interactions.CheckSocialFightStart(endConversation, otherPawn))))
        {
            if (startedFight)
            {
                socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoRecipientStarted);
            }
            else
            {
                socialFightPacks.Add(RulePackDefOfPsychology.Sentence_SocialFightConvoInitiatorStarted);
            }
            this.startedFight = true;
            if (!this.waveGoodbye && otherConvo.convoLog != null && !otherConvo.startedFight)
            {
                //The main conversation hediff was the other conversation, and didn't start a fight, so we have to add the extra sentence in after the fact.
                Traverse.Create(otherConvo.convoLog).Field("extraSentencePacks").GetValue<List<RulePackDef>>().AddRange(socialFightPacks);
            }
        }
        if (this.waveGoodbye && this.pawn.Map != null)
        {
            //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 2");
            RulePack goodbyeText = new RulePack();
            FieldInfo RuleStrings = typeof(RulePack).GetField("rulesStrings", BindingFlags.Instance | BindingFlags.NonPublic);
            List<string> text = new List<string>(1);
            text.Add("r_logentry->" + talkDesc.Translate(convoTopic, pawn.Named("INITIATOR"), otherPawn.Named("RECIPIENT")));
            RuleStrings.SetValue(goodbyeText, text);
            endConversation.logRulesInitiator = goodbyeText;
            FieldInfo Symbol = typeof(InteractionDef).GetField("symbol", BindingFlags.Instance | BindingFlags.NonPublic);
            Symbol.SetValue(endConversation, Symbol.GetValue(InteractionDefOfPsychology.EndConversation));
            PlayLogEntry_InteractionConversation log = new PlayLogEntry_InteractionConversation(endConversation, pawn, this.otherPawn, socialFightPacks);
            Find.PlayLog.Add(log);
            convoLog = log;
            MoteMaker.MakeInteractionBubble(this.pawn, this.otherPawn, InteractionDefOf.Chitchat.interactionMote, InteractionDefOf.Chitchat.GetSymbol()); // 1.3
            //Log.Message("Hediff_Conversation.PostRemoved(), pawn = " + pawn.LabelShort + " otherPawn = " + otherPawn.LabelShort + ", step 3");
        }
    }

    private ThoughtDef CreateSocialThought(out float opinionMod)
    {
        //We create a dynamic def to hold this thought so that the game won't worry about it being used anywhere else.
        ThoughtDef def = new ThoughtDef();
        def.defName = this.pawn.GetHashCode() + "Conversation" + topic.defName;
        def.label = topic.defName;
        def.durationDays = PsychologySettings.conversationDuration;
        def.nullifyingTraits = new List<TraitDef>();
        //def.nullifyingTraits.Add(TraitDefOf.Psychopath);
        def.thoughtClass = typeof(Thought_MemorySocialDynamic);
        ThoughtStage stage = new ThoughtStage();

        // NEW PSYCHOLOGY FORMULA
        float opin1 = Mathf.Clamp01(PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(topic));
        float opin2 = Mathf.Clamp01(PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(topic));

        // Baseline opinion modifier ranges from -1 to +1 and should have an expected value of +0.01.
        float controversiality = topic.controversiality;
        float opinionModRaw = PsycheHelper.SaddleShapeFunction(opin1, opin2, 1f - 0.5f * controversiality, 4f * controversiality * controversiality);
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionModRaw = " + opinionModRaw.ToString());

        float opinionModRawMin = 0.1f;
        opinionMod = 20f * (opinionModRawMin * Mathf.Sign(opinionModRaw) + (1f - opinionModRawMin) * opinionModRaw);
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMod w/o time factor = " + opinionMod.ToString());

        // Added time minimum to baseline to opinionMod because conversations often end very quickly
        opinionMod *= 1.0f + 6f * ((float)this.ageTicks / (float)(GenDate.TicksPerHour * 2.25f));
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMod w time factor = " + opinionMod.ToString());

        // Personality multiplier controls how much personality affects the opinion multiplier. Should be a positive number.
        float personalityMultiplier = 0.50f;
        // Initialize opinionMultiplier, which can be positive or negative. Represents % increase or decrease in opinionMod
        float opinionMultiplier = 0f;
        // The more judgmental the pawn, the more this affects all conversations.
        opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Judgmental));
        // All conversations are affected by how passionate and outspoken the other pawn is
        opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Passionate));
        opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Outspoken));
        // Positive opinions
        if (opinionMod > 0f)
        {
            //opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Extroverted));
            opinionMultiplier += personalityMultiplier * (+1f - 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly));
            opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Trusting));
            opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Cool));
            //opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly));
            //opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Upbeat));
            if (LovePartnerRelationUtility.LovePartnerRelationExists(this.pawn, this.otherPawn) && this.pawn.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                //If it's a positive thought about their lover, Codependent pawns are always more affected by it.
                opinionMod *= 1f + personalityMultiplier;
            }
        }
        // Negative opinions
        else
        {
            opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive));
            opinionMultiplier += personalityMultiplier * (+1f - 2f * PsycheHelper.Comp(pawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly));
            //opinionMultiplier += personalityMultiplier * (-1f + 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Aggressive));
            //opinionMultiplier += personalityMultiplier * (+1f - 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Friendly));
            opinionMultiplier += personalityMultiplier * (+1f - 2f * PsycheHelper.Comp(otherPawn).Psyche.GetPersonalityRating(PersonalityNodeDefOf.Polite));
            // In low-population colonies, pawns will put aside their differences.
            opinionMod *= PopulationModifier;
        }
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMod before multiplier = " + opinionMod.ToString());
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMultiplier = " + opinionMultiplier.ToString());

        // Multiply by 1 + opinionMultiplier for positive multiplier
        if (opinionMultiplier > 0f)
        {
            opinionMod *= 1f + opinionMultiplier;
        }
        // Divide by 1 - opinionMultiplier for negative multiplier. This ensures opinionMod will always remain positive.
        // For example, if opinionMultiplier = -2, i.e. a 200% decrease, opinionMod will be 1/3 of its original value. Note that a 200% increase of 1/3 is 1.
        else
        {
            opinionMod /= 1f - opinionMultiplier;
        }
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMod after multiplier = " + opinionMod.ToString());

        // Set minimum and maximum absolute changes in opinion.
        float opinionModMin = 5f * (1f + Rand.Value);
        float opinionModMax = 25f * (1f + Rand.Value);
        // Weaker baseline agreement/disagreement leads to lower max absolute change in opinion
        float opinionModMaxAdjusted = Mathf.Lerp(Mathf.Max(0.5f * opinionModMax, opinionModMin), opinionModMax, 2f * Mathf.Abs(opinionModRaw));
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionModMaxAdjusted = " + opinionModMaxAdjusted.ToString());

        // Opinion must change by at least +-opinionModMin and are capped at +-opinionModMaxAdjusted
        opinionMod = Mathf.Sign(opinionMod) * Mathf.Clamp(Mathf.Abs(opinionMod), opinionModMin, opinionModMaxAdjusted);
        //Log.Message(pawn.LabelShort + " and " + otherPawn.LabelShort + " opinionMod final value = " + opinionMod.ToString());

        stage.label = "ConversationStage".Translate() + " " + convoTopic;
        stage.baseOpinionOffset = Mathf.RoundToInt(opinionMod);
        def.stages.Add(stage);
        return def;
    }

    private bool TryGainThought(ThoughtDef def, int opinionOffset)
    {
        ThoughtStage stage = def.stages.First();
        IEnumerable<Thought_MemorySocialDynamic> convoMemories;
        /* The more they know about someone, the less likely small thoughts are to have an impact on their opinion.
         * This helps declutter the Social card without preventing pawns from having conversations.
         * They just won't change their mind about the colonist as a result.
         */
        if (Rand.Value < Mathf.InverseLerp(0f, PsycheHelper.Comp(pawn).Psyche.TotalThoughtOpinion(this.otherPawn, out convoMemories), 250f + Mathf.Abs(opinionOffset)) && opinionOffset != 0)
        {
            this.pawn.needs.mood.thoughts.memories.TryGainMemory(def, this.otherPawn);
            return true;
        }
        return false;
    }

    public float PopulationModifier
    {
        get
        {
            if (this.pawn.IsColonist && this.pawn.Map != null)
            {
                return Mathf.Clamp01(this.pawn.Map.mapPawns.FreeColonistsCount / 8f);
            }
            else
            {
                return 1f;
            }
        }
    }

    
}
